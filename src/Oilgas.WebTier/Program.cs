using Microsoft.EntityFrameworkCore;
using Oilgas.SqlServer;
using Oilgas.WebTier;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection(RabbitOptions.SectionName));

// читання Azure SQL для queries (pooled — HotChocolate резолвить контекст per-field)
var connectionString = builder.Configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException("ConnectionStrings:Sql не заданий (Azure SQL).");
builder.Services.AddPooledDbContextFactory<OilgasDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddHttpClient();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();

// консюмер RabbitMQ, що живить subscription
builder.Services.AddHostedService<TelemetryStreamConsumer>();

var app = builder.Build();

app.UseWebSockets();          // для GraphQL subscriptions (graphql-ws)
app.UseDefaultFiles();
app.UseStaticFiles();         // React-консоль із wwwroot
app.MapGraphQL();             // /graphql (POST для query/mutation, WS для subscription)

app.Run();
