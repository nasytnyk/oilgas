using Oilgas.Hardware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MqttOptions>(
    builder.Configuration.GetSection(MqttOptions.SectionName));
builder.Services.Configure<HardwareOptions>(
    builder.Configuration.GetSection(HardwareOptions.SectionName));
builder.Services.AddSingleton<TelemetryToggle>();
builder.Services.AddHostedService<TelemetryWorker>();

var app = builder.Build();

var telemetry = app.Services.GetRequiredService<TelemetryToggle>();

// веб-морда: статичний wwwroot/index.html подається на "/"
app.UseDefaultFiles();
app.UseStaticFiles();

// API керування потоком телеметрії
app.MapGet("/status", () => Results.Ok(new { on = telemetry.IsOn }));
app.MapPost("/start", () => { telemetry.Start(); return Results.Ok(new { on = true }); });
app.MapPost("/stop", () => { telemetry.Stop(); return Results.Ok(new { on = false }); });

app.Run();
