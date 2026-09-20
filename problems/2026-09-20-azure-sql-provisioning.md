# Провіжн БД: Azure SQL (always-free) для метрик

**Дата:** 2026-09-20
**Мета:** рідна ажурівська БД замість Neon, у межах фрі-тіру, куди db-writer пише Tick-рядки.
**Тип:** runbook — які команди виконано, щоб підняти БД (щоб було відтворювано).

> ⚠️ Реальні паролі / connection string сюди НЕ пишемо — репо публічне. Вони в GitHub Secrets
> (`SQL_CONNECTION`) і в Container Apps secret (`sql-conn`). Нижче — плейсхолдери.

---

## 1. Що вибрали і чому

**Azure SQL Database, serverless, free offer** (100k vCore-с/міс + 32GB, 1 БД на підписку,
безкоштовно **назавжди**). Обрали замість:
- Neon — поза Azure (не входить у єдиний білінг/капи);
- Postgres Flexible Server — безкоштовно лише 12 міс;
- self-host Postgres — stateful-контейнер + плата за стородж.

Ціна вибору: це SQL Server, не Postgres → EF-провайдер Npgsql замінено на SqlServer, проєкт
`Oilgas.Postgres` перейменовано на `Oilgas.SqlServer`, міграцію перегенеровано.

## 2. Реєстрація ресурс-провайдера (одноразово)

Перша спроба `az sql server create` впала:
```
(MissingSubscriptionRegistration) The subscription is not registered to use namespace 'Microsoft.Sql'
```
Підписка не мала зареєстрованого провайдера `Microsoft.Sql`. Фікс:
```bash
az provider register --namespace Microsoft.Sql
# чекати, поки стане Registered:
until [ "$(az provider show -n Microsoft.Sql --query registrationState -o tsv)" = "Registered" ]; do sleep 5; done
```

## 3. Логічний сервер SQL

```bash
SRV="oilgas-sql-<rand>"          # глобально унікальне ім'я
ADMIN="oilgasadmin"
PW="<generated-strong-password>" # у GH secret, не в git

az sql server create -n "$SRV" -g oilgas-rg -l westeurope -u "$ADMIN" -p "$PW"
```
Сервер `oilgas-sql-<rand>` → FQDN `oilgas-sql-<rand>.database.windows.net`.

## 4. Файрвол: доступ Azure-сервісам

Container Apps (наш db-writer) — це «Azure services». Спеціальне правило `0.0.0.0-0.0.0.0`
означає «дозволити доступ Azure-сервісам» (НЕ увесь інтернет):
```bash
az sql server firewall-rule create -g oilgas-rg -s "$SRV" -n AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```
Без цього db-writer діставав би timeout/refused при конекті до БД.

## 5. База даних — serverless + free limit + auto-pause

```bash
az sql db create -g oilgas-rg -s "$SRV" -n oilgas \
  --edition GeneralPurpose --compute-model Serverless --family Gen5 --capacity 2 \
  --use-free-limit --free-limit-exhaustion-behavior AutoPause
```
- `--use-free-limit` — вмикає безкоштовний місячний ліміт;
- `--free-limit-exhaustion-behavior AutoPause` — коли ліміт вичерпано, БД **ставиться на паузу**,
  а не тарифікується (альтернатива — `BillOverForUsage`, тобто доплата). Обрали AutoPause →
  **нуль зайвих центів**;
- serverless сам паузиться при простої; перший конект після паузи має затримку ~30-60с —
  тому db-writer має **ретрай на міграції/конекті** і `Connection Timeout=60`.

## 6. Connection string → секрети

Формат для .NET (Microsoft.Data.SqlClient / EF SqlServer):
```
Server=tcp:<srv>.database.windows.net,1433;Initial Catalog=oilgas;User ID=<admin>;Password=<pw>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```
Куди кладемо:
```bash
gh secret set SQL_CONNECTION -R nasytnyk/oilgas   # значення в промпт
```
Далі `deploy.yml` створює Container Apps secret `sql-conn` з цього і віддає db-writer як
env `ConnectionStrings__Sql=secretref:sql-conn`. Код читає `ConnectionStrings:Sql`.

## 7. Як EF створює схему

db-writer на старті робить `Database.MigrateAsync()` (з ретраєм) → застосовує міграцію
`InitialCreate` до Azure SQL: таблиці Mines/Devices/Ticks/Anomalies. Руками в БД нічого
створювати не треба.

## 8. Корисні команди

```bash
# показати сервер і БД
az sql server show -n <srv> -g oilgas-rg -o table
az sql db show -g oilgas-rg -s <srv> -n oilgas -o table

# правила файрволу
az sql server firewall-rule list -g oilgas-rg -s <srv> -o table

# видалити все (сервер тягне за собою БД)
az sql server delete -n <srv> -g oilgas-rg --yes
```

## 9. Урок / нотатки

1. Новій підписці треба **зареєструвати `Microsoft.Sql`** — інакше create падає.
2. `0.0.0.0-0.0.0.0` у файрволі = «Azure services», а не «весь світ» — саме це треба для
   Container Apps.
3. `AutoPause` на free limit — це і є захист «нуль центів» на рівні самої БД.
4. Serverless паузиться → перший запит холодний; ретрай на старті обов'язковий.
