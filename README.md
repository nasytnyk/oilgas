# Oilgas — адмін-дашборд

Чорновий набір посилань для керування демо. Далі доповнюватимемо.

## Швидкі посилання

| Що | Посилання |
|---|---|
| webmorda | <https://oilgas-hardware.livelyplant-0a492aef.westeurope.azurecontainerapps.io/> |
| mosquitto | internal TCP `oilgas-mosquitto:1883` [resource](https://portal.azure.com/#@nasytnykgmail.onmicrosoft.com/resource/subscriptions/b6bc393e-1ed0-46f0-9a4b-f2dd06f06622/resourceGroups/oilgas-rg/providers/Microsoft.App/containerApps/oilgas-mosquitto/overview) |
| ⏻ Power OFF / ON (усі Container Apps у RG) | [Actions → Power → Run workflow](https://github.com/nasytnyk/oilgas/actions/workflows/power.yml) |
| 📦 Ресурсна група `oilgas-rg` | [Azure Portal](https://portal.azure.com/#@nasytnykgmail.onmicrosoft.com/resource/subscriptions/b6bc393e-1ed0-46f0-9a4b-f2dd06f06622/resourceGroups/oilgas-rg/overview) |

## Power OFF / ON

Строгий стоп/старт цілої RG (повна деалокація → компьют $0, коли вимкнено; сам застосунок не видаляється).

Через UI: [Actions → Power → Run workflow](https://github.com/nasytnyk/oilgas/actions/workflows/power.yml) → обрати `stop` або `start`.

Через CLI:

```bash
gh workflow run power.yml -f action=stop  -R nasytnyk/oilgas
```

```bash
gh workflow run power.yml -f action=start -R nasytnyk/oilgas
```

> Після `start` телеметрія піднімається у стані **OFF** — вмикається кнопкою на веб-морді (`TelemetrySwitch` тримається в пам'яті процесу).

## Довідка

- Деплой: push у `main` → воркфлоу [Deploy](.github/workflows/deploy.yml) (build → GHCR → Container Apps).
- Runbook: [AZURE-INIT.md](AZURE-INIT.md).
- Розбір мережевої проблеми: [problems/2026-09-19-mqtt-internal-ingress.md](problems/2026-09-19-mqtt-internal-ingress.md).
