# Azure Init — деплой Oilgas (Mosquitto + Hardware)

Runbook під **поточну** ситуацію: self-hosted **Mosquitto** (брокер) + **Hardware** (симулятор із веб-мордою on/off) у **Azure Container Apps**, збірка/деплой через **GitHub Actions (OIDC + GHCR)**.

> ⚠️ Реальні ID (subscription/tenant/client) сюди не пишемо — репо публічне. Вони в GitHub Secrets.

## Рішення
- Регіон: **West Europe**; RG: **oilgas-rg**; env: **oilgas-env**
- Брокер: **Mosquitto**, окремий Container App, **internal TCP** ingress :1883
- Hardware: окремий Container App, **external** ingress :8080 (веб-морда + `/start` `/stop`)
- Образи: **GHCR** (public); авт-ція GitHub→Azure: **OIDC federated**
- **Мережевий нюанс:** internal TCP-застосунок доступний **за іменем** (`oilgas-mosquitto`), не за FQDN — див. `problems/2026-09-19-mqtt-internal-ingress.md`

## Передумови
- `az`, `gh` (авторизований), Docker; `az login` виконано; RG `oilgas-rg` створено.

## Крок 1 — Container Apps environment
```bash
az containerapp env create -n oilgas-env -g oilgas-rg -l westeurope
```

## Крок 2 — OIDC (GitHub Actions → Azure)
```bash
APP_ID=$(az ad app create --display-name oilgas-github --query appId -o tsv)
az ad sp create --id "$APP_ID"
SP_OID=$(az ad sp show --id "$APP_ID" --query id -o tsv)
SUB_ID=$(az account show --query id -o tsv)
az role assignment create --assignee-object-id "$SP_OID" --assignee-principal-type ServicePrincipal \
  --role Contributor --scope "/subscriptions/$SUB_ID/resourceGroups/oilgas-rg"

# federated credentials: login-форма + ID-форма (цей акаунт віддає subject у формі ID)
read OWNER_ID REPO_ID < <(gh api repos/nasytnyk/oilgas --jq '"\(.owner.id) \(.id)"')
az ad app federated-credential create --id "$APP_ID" --parameters "{\"name\":\"github-main\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:nasytnyk/oilgas:ref:refs/heads/main\",\"audiences\":[\"api://AzureADTokenExchange\"]}"
az ad app federated-credential create --id "$APP_ID" --parameters "{\"name\":\"github-main-idform\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:nasytnyk@${OWNER_ID}/oilgas@${REPO_ID}:ref:refs/heads/main\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

gh secret set AZURE_CLIENT_ID       -b "$APP_ID"                                    -R nasytnyk/oilgas
gh secret set AZURE_TENANT_ID       -b "$(az account show --query tenantId -o tsv)" -R nasytnyk/oilgas
gh secret set AZURE_SUBSCRIPTION_ID -b "$SUB_ID"                                    -R nasytnyk/oilgas
```

## Крок 3 — Образи + деплой = CI/CD
Далі все робить `.github/workflows/deploy.yml` на кожен push у `main`:
1. збирає образи `oilgas-hardware` і `oilgas-mosquitto`, пушить у GHCR;
2. `azure/login` (OIDC);
3. create-or-update **Mosquitto** (internal TCP :1883) і **Hardware** (external :8080, `Mqtt__Host=oilgas-mosquitto`).

**Одноразово:** зробити GHCR-пакети `oilgas-hardware` і `oilgas-mosquitto` **public** (Package settings → Change visibility), щоб Container Apps тягнув без кредів.

Результат: веб-морда Hardware на `https://oilgas-hardware.<env-domain>/` — кнопками вмикаєш/вимикаєш симуляцію; телеметрія тече в Mosquitto.

## Ручний деплой (якщо треба без CD)
```bash
# Mosquitto (internal TCP)
az containerapp create -n oilgas-mosquitto -g oilgas-rg --environment oilgas-env \
  --image ghcr.io/nasytnyk/oilgas-mosquitto:latest \
  --ingress internal --transport tcp --target-port 1883 --exposed-port 1883 \
  --min-replicas 1 --max-replicas 1 --cpu 0.25 --memory 0.5Gi

# Hardware (external) — BrokerHost = ІМ'Я застосунку Mosquitto
az containerapp create -n oilgas-hardware -g oilgas-rg --environment oilgas-env \
  --image ghcr.io/nasytnyk/oilgas-hardware:latest \
  --ingress external --target-port 8080 \
  --min-replicas 1 --max-replicas 1 --cpu 0.25 --memory 0.5Gi \
  --env-vars Mqtt__Host=oilgas-mosquitto Mqtt__Port=1883 Hardware__IntervalSeconds=5
```

## Знесення
```bash
az group delete --name oilgas-rg --yes --no-wait
# Entra app окремо (не в RG):
# az ad app delete --id <AZURE_CLIENT_ID>
```
