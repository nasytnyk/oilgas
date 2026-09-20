using Oilgas.RawLog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection(RabbitOptions.SectionName));
var capacity = builder.Configuration.GetSection(RabbitOptions.SectionName).Get<RabbitOptions>()?.Capacity ?? 200;
builder.Services.AddSingleton(new RawLogStore(capacity));
builder.Services.AddHostedService<RawLogConsumer>();

var app = builder.Build();

var store = app.Services.GetRequiredService<RawLogStore>();

app.UseDefaultFiles();
app.UseStaticFiles();

// останні повідомлення (найновіші зверху) для веб-морди
app.MapGet("/messages", () => Results.Ok(store.Snapshot()));

app.Run();
