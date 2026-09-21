# Як зроблено моніторинг споживання RG зі скрипта (без порталу)

**Артефакт:** `scripts/status.sh`
**Мета:** бачити вартість, стан апів, залишок free grant і метрики БД командою, а не кліками.

---

## 1. Задача

Треба «по-програмістськи» відповісти на три питання по `oilgas-rg`:
1. скільки коштує (за місяць, по ресурсах);
2. що зараз запущено (і скільки реплік);
3. чи не з'їдаємо free grant Container Apps.

## 2. Глухі кути (і чому вони важливі)

### 2.1 `az costmanagement query` — немає
Очевидний шлях — Cost Management query API. Але розширення `costmanagement` у цій версії CLI
має лише `export`, і `az costmanagement query` падає:
```
'query' is misspelled or not recognized by the system.
```
**Фікс:** вбудований модуль `az consumption usage list` (у preview, дає warning, але працює).
Він не має параметра scope=RG, тож фільтруємо по `instanceName` через `jq`:
```bash
az consumption usage list -o json \
  | jq -r --arg rg "/resourcegroups/oilgas-rg/" '
      [.[] | select((.instanceName//""|ascii_downcase)|contains($rg))] as $r
      | if ($r|length)==0 then "  (ще нема тарифікованих записів)"
        else ($r|group_by(.product)[]
              | "  \(.[0].product): $\([.[].pretaxCost|tonumber? // 0]|add)") end'
```
Нюанс: `pretaxCost` буває `"None"` (рядок) → `tonumber? // 0` це поглинає.

### 2.2 Free grant Container Apps — Azure НЕ віддає фактично спожите
Хотілося «спожито X% зі 180k vCPU-секунд». Але:
- фрі-грант ACA **не деталізується** в `az consumption usage list` (усе $0 → записів по Microsoft.App
  для групи просто нема);
- окремого «free-grant-used %» ендпоінта немає.

**Рішення — рахувати не ретроспективу, а capacity:** скільки з'їси, якщо тримати запущене 24/7
весь місяць. Беремо алокацію кожного *running*-апа (`resources.cpu`/`memory`) і множимо на
секунди місяця; порівнюємо з лімітами (180k vCPU-с, 360k GiB-с):
```bash
VCPU_FREE=180000; MONTH_SECS=2592000; rate_cpu=0; running=0
while read -r app; do
  st=$(az containerapp show -n "$app" -g oilgas-rg --query "properties.runningStatus" -o tsv)
  [ "$st" = "Running" ] || continue
  cpu=$(az containerapp show -n "$app" -g oilgas-rg --query "properties.template.containers[0].resources.cpu" -o tsv)
  running=$((running+1)); rate_cpu=$(echo "$rate_cpu + $cpu" | bc -l)
done < <(az containerapp list -g oilgas-rg --query "[].name" -o tsv)
# % free grant за 24/7 і скільки годин вистачає:
echo "scale=1; $rate_cpu*$MONTH_SECS/$VCPU_FREE*100" | bc -l
echo "scale=1; $VCPU_FREE/($rate_cpu*3600)" | bc -l
```
Це **чесна оцінка потужності**, а не факт: якщо стопиш/стартуєш — реально з'їдено менше. Але саме
capacity-число корисне («на скільки годин вистачить»), і воно одразу показало: **6 апів × 1.75 vCPU
24/7 = ~2520% ліміту, тобто free grant вистачає лише на ~28 год/міс**. Звідси висновок — Power stop.

### 2.3 Метрики Azure SQL — навпаки, легко
Тут проблеми не було: `az monitor metrics list` дає near-real-time метрики ресурсу:
```bash
DBID="/subscriptions/<sub>/resourceGroups/oilgas-rg/providers/Microsoft.Sql/servers/<srv>/databases/oilgas"
az monitor metrics list --resource "$DBID" --metric cpu_percent storage_percent \
  --interval PT5M --aggregation Average \
  --query "value[].{metric:name.value, last:timeseries[0].data[-1].average}" -o table
```
Ім'я сервера не хардкодимо — дістаємо `az sql server list -g oilgas-rg --query "[0].name"`.

## 3. Що зібралось у скрипт

`scripts/status.sh` = чотири блоки: вартість (consumption+jq), стан апів (containerapp list),
capacity free grant (розрахунок на bc), метрики SQL (monitor metrics). Залежності: `az`, `jq`, `bc`.

## 4. Побічне рішення: paths-ignore у CI
Щоб коміти в `scripts/**`, `problems/**`, `**.md` не запускали деплой, додали в `deploy.yml`:
```yaml
on:
  push:
    branches: [ main ]
    paths-ignore: [ '**.md', 'scripts/**', 'problems/**' ]
```

## 5. Уроки

1. `costmanagement query` не завжди є — `az consumption usage list` + `jq` надійніший, хоч і preview.
2. Free grant ACA Azure не показує фактично — рахуй **capacity** (алокація × час), це дає
   actionable «на скільки вистачить».
3. Метрики ресурсів (SQL, ACA) — через `az monitor metrics list`, near-real-time, без порталу.
4. Не хардкодь імена ресурсів — діставай через `az ... list --query`.
