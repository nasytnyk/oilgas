using Microsoft.EntityFrameworkCore;
using Oilgas.RmqToDb;
using Oilgas.Ef;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection(RabbitOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException("ConnectionStrings:Sql не заданий (Azure SQL connection string).");
builder.Services.AddDbContext<OilgasDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddHostedService<DbWriterConsumer>();

builder.Build().Run();
