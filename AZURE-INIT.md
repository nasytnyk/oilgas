# Azure Init — покрокова ініціалізація деплою Oleumetry

Runbook, що піднімає інфраструктуру для деплою `WebTier` у **Azure Container Apps** через **GitHub Actions (OIDC)**.
Кожен крок — команда + пояснення *навіщо*. Можна виконати повторно з нуля.

> ⚠️ Реальні ID (subscription / tenant / client) сюди **не записуються** — репозиторій публічний.
> Вони живуть лише в GitHub Secrets. Тут скрізь плейсхолдери.

## Зафіксовані рішення
- Регіон: **West Europe**
- Реєстр образів: **GHCR** (публічні образи — репо публічне, тягнуться без кредів)
- Автентифікація GitHub→Azure: **OIDC federated** (без довгоживучих секретів)
- Перший деплой: **WebTier** (walking skeleton)

## Передумови
- `az`, `gh` (авторизований), Docker — встановлені.
- `az login` виконано; активна підписка видно через `az account show`.

## Змінні (для команд нижче)
```bash
RG=oleumetry-rg
LOCATION=westeurope
ENV=oleumetry-env
APP=oleumetry-webtier
GH_REPO=nasytnyk/oleumetry
APP_REG=oleumetry-github          # Entra ID app для OIDC
```

---

## Крок 1 — Resource group
**Що це:** логічний контейнер для всіх ресурсів проекту. Дає одну точку керування і видалення (`az group delete -n oleumetry-rg` знесе все разом).

```bash
az group create --name $RG --location $LOCATION
```

**Навіщо саме так:** усі подальші ресурси (Container Apps environment, застосунок) створюються всередині цієї RG у West Europe.

---

## Крок 2 — Container Apps environment
**Що це:** середовище виконання для Container Apps — спільна межа мережі/логів, у якій живуть застосунки-контейнери. За замовчуванням створює Log Analytics workspace для збору логів.

```bash
# розширення az CLI для Container Apps (одноразово)
az extension add --name containerapp --upgrade

# реєстрація постачальників ресурсів (одноразово на підписку)
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.OperationalInsights --wait

# саме середовище (план Consumption — має безкоштовний грант)
az containerapp env create --name $ENV --resource-group $RG --location $LOCATION
```

**Навіщо:** застосунок `WebTier` деплоїться *в* environment. Реєстрація провайдерів (`Microsoft.App` — Container Apps; `Microsoft.OperationalInsights` — логи) потрібна раз на підписку.

**Результат:** environment `oleumetry-env` — стан `Succeeded`; отримує публічний домен виду `<random>.westeurope.azurecontainerapps.io`, під яким будуть доступні застосунки.

---

## Крок 3 — OIDC (GitHub Actions → Azure без секретів-паролів)
**Що це:** щоб workflow міг деплоїти, йому потрібні права в Azure. Замість довгоживучого пароля використовуємо **federated credential**: GitHub видає короткоживучий OIDC-токен, а Azure довіряє йому за збігом `issuer` + `subject`.

```bash
# 3a. App registration в Entra ID
APP_ID=$(az ad app create --display-name oleumetry-github --query appId -o tsv)

# 3b. Service principal для цього app
az ad sp create --id "$APP_ID"

# 3c. Роль Contributor, обмежена ОДНІЄЮ resource group (least privilege)
SUB_ID=$(az account show --query id -o tsv)
az role assignment create --assignee "$APP_ID" --role Contributor \
  --scope "/subscriptions/$SUB_ID/resourceGroups/oleumetry-rg"

# 3d. Federated credential: довіра до workflow саме з нашого репо+гілки
az ad app federated-credential create --id "$APP_ID" --parameters '{
  "name":"github-main",
  "issuer":"https://token.actions.githubusercontent.com",
  "subject":"repo:nasytnyk/oleumetry:ref:refs/heads/main",
  "audiences":["api://AzureADTokenExchange"]
}'

# 3e. Кладемо ID у GitHub Secrets (це ідентифікатори, не паролі)
gh secret set AZURE_CLIENT_ID       -b "$APP_ID" -R nasytnyk/oleumetry
gh secret set AZURE_TENANT_ID       -b "$(az account show --query tenantId -o tsv)" -R nasytnyk/oleumetry
gh secret set AZURE_SUBSCRIPTION_ID -b "$SUB_ID" -R nasytnyk/oleumetry
```

**Навіщо:** `subject` прив'язує довіру саме до `nasytnyk/oleumetry` + `main` — OIDC-токен з іншого репо/гілки не підійде. У workflow `azure/login` обміняє OIDC-токен на короткоживучий Azure-токен — **жодного пароля в GitHub**.

**Результат:** app `oleumetry-github` + SP + Contributor на `oleumetry-rg` + federated credential (`main`); секрети `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` у репо.

---

## Крок 4 — Dockerfile для WebTier
**Що це:** інструкція збірки образу. Багатоетапна: етап `build` (повний SDK) компілює й `publish`-ить; етап `runtime` (легкий `aspnet`-образ) містить лише готовий застосунок — менший і безпечніший.

- Файл: `src/Oleumetry.WebTier/Dockerfile`. Контекст збірки — **корінь репозиторію** (щоб бачити `global.json` і граф проектів).
- Порт **8080** (ASP.NET у контейнері за замовчуванням слухає 8080; Container Apps проксить на нього).
- `.dockerignore` у корені виключає `bin/`, `obj/`, `.git/` з контексту.

## Крок 5 — Workflow: build + push у GHCR
Файл: `.github/workflows/deploy.yml`. На push у `main` (зміни в `src/**`):
1. `docker/login-action` логіниться в GHCR вбудованим `GITHUB_TOKEN` (живе лише під час запуску);
2. `docker/build-push-action` збирає образ і пушить теги `:<sha>` та `:latest`.

**Навіщо `packages: write`:** право пушу образу в GHCR. **Чому тут ще нема деплою:** свіжий пакет GHCR приватний — крок деплою додамо після того, як зробимо його public (Крок 6).
