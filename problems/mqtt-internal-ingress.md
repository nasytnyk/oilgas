# Проблема: Hardware не міг достукатись до Mosquitto в Azure Container Apps

**Компоненти:** `oilgas-hardware` (MQTT-клієнт, MQTTnet) → `oilgas-mosquitto` (брокер, eclipse-mosquitto), обидва — Azure Container Apps в одному environment `oilgas-env`.

---

## 1. Симптом

Після деплою двох окремих Container Apps:
- `oilgas-mosquitto` — **internal** ingress, `--transport tcp --target-port 1883 --exposed-port 1883`;
- `oilgas-hardware` — **external** ingress :8080, env `Hardware__BrokerHost=<повний internal FQDN Mosquitto>`.

Веб-морда Hardware (`/status`, `/start`) працювала, але **телеметрія не текла**. У логах Hardware:

```
warn: Oilgas.Hardware.TelemetryWorker[0]
      Connect esp-001 attempt 1 failed: The operation has timed out.
```

Тобто MQTT-клієнт не міг встановити TCP-з'єднання з брокером. Це повторювалось після рестарту й після того, як Mosquitto вже був `Running` — отже **не стартова гонка**.

---

## 2. Як діагностував (крок за кроком)

### 2.1 «Слухав трафік» на брокері — логи Mosquitto
Найінформативніше — дивитись, ХТО підключається до брокера. Mosquitto логує кожен конект:

```bash
az containerapp logs show -n oilgas-mosquitto -g oilgas-rg --tail 40
```

Бачив лише короткоживучі з'єднання з `127.0.0.1`, що одразу закриваються:
```
New connection from 127.0.0.1:33608 on port 1883.
Client 127.0.0.1 ... disconnected: connection closed by client.
```
Це **TCP health-проби** самого ingress (envoy-сайдкар у тому ж поді). А от рядків **`New client connected ... as oilgas-hw-*`** (наших MQTT-клієнтів) НЕ було. Висновок: трафік Hardware до брокера не доходив узагалі.

### 2.2 Логи застосунку Hardware
Container Apps віддає логи через **Log Analytics із затримкою ~1–2 хв** — тому свіжі рядки не одразу видно. Плюс веб-морда, що опитує `/status`, флудить логи HTTP-запитами. Фільтрував не-HTTP-рядки, щоб дістати рядки воркера:

```bash
az containerapp logs show -n oilgas-hardware -g oilgas-rg --tail 300 \
  | python3 -c "import sys,json
skip=('Hosting.Diagnostics','EndpointMiddleware','OkObjectResult','Request starting','Request finished')
for l in sys.stdin:
  l=l.strip()
  if not l: continue
  try: m=json.loads(l).get('Log',l)
  except: m=l
  if any(s in m for s in skip): continue
  print(m)"
```
Так і виловив `Connect esp-001 attempt 1 failed: The operation has timed out.`

### 2.3 Спроба живого стріму (реалтайм, без Log Analytics)
```bash
az containerapp logs show -n oilgas-hardware -g oilgas-rg --follow --tail 20
```
`--follow` дає живий консоль-стрім без затримки LA. **Але** в неінтерактивній (без TTY) сесії, обгорнутий у `timeout ... | grep | head`, він губив вивід (буферизація пайпів + SIGTERM). Урок: `--follow` добре працює в інтерактивному терміналі; у скриптах — ненадійно. Для реалтайму краще: відкрити веб-морду / **Log stream у Azure Portal**, або читати логи брокера (див. 2.1).

### 2.4 Перевірка конфігурації (відкинути тривіальні причини)
Env-змінні на контейнері Hardware:
```bash
az containerapp show -n oilgas-hardware -g oilgas-rg \
  --query "properties.template.containers[0].env" -o json
```
→ `Hardware__BrokerHost` = повний FQDN, порт 1883 — виглядало «правильно».

Ingress брокера:
```bash
az containerapp show -n oilgas-mosquitto -g oilgas-rg \
  --query "properties.configuration.ingress" -o json
```
→ `transport: Tcp`, `exposedPort: 1883`, `targetPort: 1883`, `external: false`, `fqdn: oilgas-mosquitto.internal.<env>...` — теж «правильно».

### 2.5 Документація → root cause
Оскільки конфіг виглядав коректним, пішов у доку:
```bash
# WebSearch + WebFetch офіційної сторінки
# https://learn.microsoft.com/azure/container-apps/ingress-overview
```
Ключове формулювання (розділ **TCP**):
> *An app with TCP ingress is accessible to other container apps in the same environment via its **name** (the `name` property) and exposed port number.*

І окремо:
> *External TCP ingress is only supported for environments that use a virtual network.*

---

## 3. Root cause

**Internal TCP-ingress у Container Apps доступний за ІМЕНЕМ застосунку, а не за повним internal FQDN.**
Я підключався за FQDN `oilgas-mosquitto.internal.<env>.azurecontainerapps.io:1883` → маршрут для TCP через FQDN не піднімається → **TCP-таймаут**.

Додатково:
- Internal **HTTP** ingress працює і за іменем, і за FQDN — тому веб-частини (WebTier, веб-морда Hardware) не мали проблем.
- **External** TCP ingress додатково вимагав би VNet-integrated environment; **internal** TCP — ні.

---

## 4. Фікс

Змінити `BrokerHost` з FQDN на **ім'я застосунку**:

```bash
az containerapp update -n oilgas-hardware -g oilgas-rg \
  --set-env-vars "Hardware__BrokerHost=oilgas-mosquitto"
```

## 5. Перевірка після фіксу

Логи Mosquitto одразу показали справжні MQTT-сесії:
```
New client connected from 100.100.0.14:33108 as oilgas-hw-wh-001 (p5, c1, k15).
New client connected from 100.100.0.128:43588 as oilgas-hw-sep-001 (p5, c1, k15).
New client connected from 100.100.0.14:33122 as oilgas-hw-cmp-001 (p5, c1, k15).
```
Логи Hardware: `Hardware connected: 4 units`. Проблему усунено.

---

## 6. Довідник команд

```bash
# стан застосунку
az containerapp show -n <app> -g oilgas-rg --query "properties.runningStatus" -o tsv

# логи (через Log Analytics, із затримкою)
az containerapp logs show -n <app> -g oilgas-rg --tail 200

# живий стрім (реалтайм; краще в інтерактивному терміналі)
az containerapp logs show -n <app> -g oilgas-rg --follow

# ingress-конфіг / env-змінні
az containerapp show -n <app> -g oilgas-rg --query "properties.configuration.ingress" -o json
az containerapp show -n <app> -g oilgas-rg --query "properties.template.containers[0].env" -o json

# оновити env → нова ревізія
az containerapp update -n <app> -g oilgas-rg --set-env-vars "KEY=VALUE"
```

## 7. Уроки

1. **Internal TCP між Container Apps — конектитись за іменем застосунку, не за FQDN.** (Записано в пам'ять асистента.)
2. **Логи брокера — найкращий «сніфер»**: показують, чи клієнт реально під'єднався (`New client connected as ...`), на відміну від логів застосунку, які лагають через Log Analytics.
3. Health-проби ingress виглядають як конекти з `127.0.0.1`, що одразу закриваються — це норма, не плутати з клієнтами.
4. Для реального realtime — Portal **Log stream** або самі логи брокера; CLI `--follow` ненадійний у неінтерактивних пайпах.
