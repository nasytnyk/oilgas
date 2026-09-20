#!/usr/bin/env bash
# Дашборд ресурсної групи oilgas-rg без порталу: вартість, стан апів,
# оцінка free grant Container Apps і метрики Azure SQL. Потребує: az (залогінений), jq, bc.
set -euo pipefail

RG=oilgas-rg
SUB=$(az account show --query id -o tsv)

echo "== Вартість за місяць (по продуктах, лише $RG) =="
# costmanagement query у CLI тут відсутній -> вбудований consumption (preview, але робочий)
az consumption usage list -o json 2>/dev/null \
  | jq -r --arg rg "/resourcegroups/${RG}/" '
      [.[] | select((.instanceName//""|ascii_downcase)|contains($rg))] as $r
      | if ($r|length)==0 then "  (ще нема тарифікованих записів)"
        else ($r|group_by(.product)[]
              | "  \(.[0].product): $\([.[].pretaxCost|tonumber? // 0]|add)") end'

echo
echo "== Container Apps: стан і репліки =="
az containerapp list -g "$RG" \
  --query "[].{app:name, status:properties.runningStatus, min:properties.template.scale.minReplicas}" -o table

echo
echo "== Container Apps free grant (оцінка за ПОТОЧНОЇ потужності) =="
# Azure не віддає фактично спожитий free grant для ACA -> рахуємо capacity:
# скільки з'їси, якщо тримати запущене 24/7 весь місяць. Ліміти free grant:
VCPU_FREE=180000; MEM_FREE=360000; MONTH_SECS=2592000
rate_cpu=0; rate_mem=0; running=0
while read -r app; do
  st=$(az containerapp show -n "$app" -g "$RG" --query "properties.runningStatus" -o tsv)
  [ "$st" = "Running" ] || continue
  cpu=$(az containerapp show -n "$app" -g "$RG" --query "properties.template.containers[0].resources.cpu" -o tsv)
  mem=$(az containerapp show -n "$app" -g "$RG" --query "properties.template.containers[0].resources.memory" -o tsv); mem=${mem%Gi}
  running=$((running+1)); rate_cpu=$(echo "$rate_cpu + $cpu" | bc -l); rate_mem=$(echo "$rate_mem + $mem" | bc -l)
done < <(az containerapp list -g "$RG" --query "[].name" -o tsv)
if [ "$running" -gt 0 ]; then
  pct_cpu=$(echo "scale=1; $rate_cpu*$MONTH_SECS/$VCPU_FREE*100" | bc -l)
  pct_mem=$(echo "scale=1; $rate_mem*$MONTH_SECS/$MEM_FREE*100" | bc -l)
  hrs=$(echo "scale=1; $VCPU_FREE/($rate_cpu*3600)" | bc -l)
  printf "  Running: %s апів, %s vCPU / %s GiB\n" "$running" "$rate_cpu" "$rate_mem"
  printf "  24/7 весь місяць => vCPU ~%s%%, RAM ~%s%% free grant\n" "$pct_cpu" "$pct_mem"
  printf "  free grant вистачає на ~%s год/міс роботи за цієї потужності\n" "$hrs"
else
  echo "  усі апи зупинені — 0 споживання (добре для економії)"
fi

echo
echo "== Azure SQL: CPU / стородж (останнє) =="
SRV=$(az sql server list -g "$RG" --query "[0].name" -o tsv 2>/dev/null || true)
if [ -n "${SRV:-}" ]; then
  DBID="/subscriptions/$SUB/resourceGroups/$RG/providers/Microsoft.Sql/servers/$SRV/databases/oilgas"
  az monitor metrics list --resource "$DBID" --metric cpu_percent storage_percent \
    --interval PT5M --aggregation Average \
    --query "value[].{metric:name.value, last:timeseries[0].data[-1].average}" -o table
else
  echo "  (SQL-сервер у $RG не знайдено)"
fi
