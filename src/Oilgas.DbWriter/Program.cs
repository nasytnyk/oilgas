using Microsoft.EntityFrameworkCore;
using Oilgas.DbWriter;
using Oilgas.Postgres;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection(RabbitOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres не заданий (Neon connection string).");
builder.Services.AddDbContext<OilgasDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddHostedService<DbWriterConsumer>();

builder.Build().Run();
