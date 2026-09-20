using Oilgas.MqttToRmq;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection(MqttOptions.SectionName));
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection(RabbitOptions.SectionName));
builder.Services.AddHostedService<BridgeWorker>();

builder.Build().Run();
