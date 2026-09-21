using Oilgas.Hardware;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection(MqttOptions.SectionName));
builder.Services.Configure<HardwareOptions>(builder.Configuration.GetSection(HardwareOptions.SectionName));

builder.Services.AddSingleton<IReadOnlyList<HardwareUnit>>(HardwareRoster.Build());
builder.Services.AddSingleton<TelemetryToggle>();
builder.Services.AddHostedService<TelemetryWorker>();    // публікує телеметрію в MQTT
builder.Services.AddHostedService<ControlListener>();    // слухає команду on/off з MQTT

builder.Build().Run();
