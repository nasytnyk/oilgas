namespace Oilgas.WebTier;

/// <summary>GraphQL mutations — керування пайплайном (проксі на Hardware).</summary>
public sealed class Mutation
{
    /// <summary>Увімкнути/вимкнути телеметрію: проксі на /start|/stop Hardware.</summary>
    public async Task<bool> SetTelemetry(
        bool on, IHttpClientFactory factory, IConfiguration cfg, CancellationToken ct)
    {
        var baseUrl = cfg["Hardware:BaseUrl"] ?? "http://oilgas-hardware";
        var client = factory.CreateClient();
        var resp = await client.PostAsync($"{baseUrl}/{(on ? "start" : "stop")}", null, ct);
        return resp.IsSuccessStatusCode;
    }
}
