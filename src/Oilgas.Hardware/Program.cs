using Oilgas.Hardware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HardwareOptions>(
    builder.Configuration.GetSection(HardwareOptions.SectionName));
builder.Services.AddSingleton<TelemetrySwitch>();
builder.Services.AddHostedService<TelemetryWorker>();

var app = builder.Build();

var telemetry = app.Services.GetRequiredService<TelemetrySwitch>();

// HTTP-контроль потоку телеметрії (дефолт OFF)
app.MapGet("/", () => "Oilgas Hardware — POST /start, POST /stop, GET /status");
app.MapGet("/status", () => Results.Ok(new { on = telemetry.IsOn }));
app.MapPost("/start", () => { telemetry.Start(); return Results.Ok(new { on = true }); });
app.MapPost("/stop", () => { telemetry.Stop(); return Results.Ok(new { on = false }); });

app.Run();
