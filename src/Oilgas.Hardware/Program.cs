using Oilgas.Hardware;
using Oilgas.Model;

var builder = WebApplication.CreateBuilder(args);

// data-driven серце: метрики і «поле» вантажаться з JSON поряд із кодом (копії у виводі).
var baseDir = AppContext.BaseDirectory;
builder.Configuration
    .AddJsonFile(Path.Combine(baseDir, "measurements.json"), optional: false, reloadOnChange: false)
    .AddJsonFile(Path.Combine(baseDir, "hardware.json"), optional: false, reloadOnChange: false);

builder.Services.Configure<MqttOptions>(
    builder.Configuration.GetSection(MqttOptions.SectionName));
builder.Services.Configure<HardwareOptions>(
    builder.Configuration.GetSection(HardwareOptions.SectionName));

// реєстр метрик з measurements.json
var measurementDefs = builder.Configuration.GetSection("Measurements").Get<List<MeasurementDefinition>>()
    ?? throw new InvalidOperationException("measurements.json: секція 'Measurements' відсутня.");
builder.Services.AddSingleton(new MeasurementRegistry(measurementDefs));

// набір пристроїв із hardware.json (резолвиться через реєстр метрик)
var deviceDefs = builder.Configuration.GetSection("Devices").Get<List<DeviceDefinition>>()
    ?? throw new InvalidOperationException("hardware.json: секція 'Devices' відсутня.");
builder.Services.AddSingleton(sp =>
    HardwareFactory.Build(deviceDefs, sp.GetRequiredService<MeasurementRegistry>()));

builder.Services.AddSingleton<TelemetryToggle>();
builder.Services.AddHostedService<TelemetryWorker>();

var app = builder.Build();

var telemetry = app.Services.GetRequiredService<TelemetryToggle>();

// Тільки внутрішнє контрольне API (без сторінки для юзера) — вимикач живе в Ui,
// який смикає ці ендпоінти. Hardware = «залізо» без морди, internal ingress.
app.MapPost("/start", () => { telemetry.Start(); return Results.Ok(new { on = true }); });
app.MapPost("/stop", () => { telemetry.Stop(); return Results.Ok(new { on = false }); });

app.Run();
