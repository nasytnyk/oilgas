using Oleumetry.Devices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SimulatorOptions>(
    builder.Configuration.GetSection(SimulatorOptions.SectionName));
builder.Services.AddHostedService<SimulatorWorker>();

var host = builder.Build();
host.Run();
