namespace Oilgas.Hardware;

/// <summary>Мінімальна веб-морда: кнопки Start/Stop + живий статус телеметрії.</summary>
internal static class Ui
{
    public const string Page = """
    <!doctype html>
    <html lang="uk">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1">
      <title>Oilgas Hardware</title>
      <style>
        body { font-family: system-ui, sans-serif; max-width: 520px; margin: 48px auto; padding: 0 16px; color: #111; }
        h1 { font-size: 22px; }
        #status { font-size: 20px; margin: 20px 0; display: flex; align-items: center; gap: 10px; }
        .dot { width: 14px; height: 14px; border-radius: 50%; background: #9ca3af; }
        button { font-size: 16px; padding: 12px 22px; margin-right: 10px; border: 0; border-radius: 10px; cursor: pointer; color: #fff; }
        .start { background: #16a34a; } .stop { background: #dc2626; }
        .muted { color: #6b7280; font-size: 13px; margin-top: 24px; }
      </style>
    </head>
    <body>
      <h1>Oilgas — симуляція обладнання</h1>
      <div id="status"><span class="dot" id="dot"></span><span id="text">…</span></div>
      <button class="start" onclick="act('start')">▶ Увімкнути</button>
      <button class="stop" onclick="act('stop')">⏹ Вимкнути</button>
      <p class="muted">Пристрої публікують телеметрію в MQTT, лише коли увімкнено.</p>
      <script>
        async function refresh() {
          try {
            const j = await (await fetch('status')).json();
            document.getElementById('text').textContent = j.on ? 'Телеметрія тече' : 'Вимкнено';
            document.getElementById('dot').style.background = j.on ? '#16a34a' : '#9ca3af';
          } catch { document.getElementById('text').textContent = 'недоступно'; }
        }
        async function act(a) { await fetch(a, { method: 'POST' }); refresh(); }
        refresh(); setInterval(refresh, 3000);
      </script>
    </body>
    </html>
    """;
}
