using Oleumetry.Devices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceOptions>(
    builder.Configuration.GetSection(DeviceOptions.SectionName));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
