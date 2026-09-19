using Oilgas.Hardware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HardwareOptions>(
    builder.Configuration.GetSection(HardwareOptions.SectionName));
builder.Services.AddSingleton<TelemetrySwitch>();
builder.Services.AddHostedService<TelemetryWorker>();

var app = builder.Build();

var telemetry = app.Services.GetRequiredService<TelemetrySwitch>();

// API керування потоком телеметрії
app.MapGet("/status", () => Results.Ok(new { on = telemetry.IsOn }));
app.MapPost("/start", () => { telemetry.Start(); return Results.Ok(new { on = true }); });
app.MapPost("/stop", () => { telemetry.Stop(); return Results.Ok(new { on = false }); });

// веб-морда з кнопками
app.MapGet("/", () => Results.Content(Ui.Page, "text/html; charset=utf-8"));

app.Run();
