using Microsoft.Extensions.Configuration;

namespace ModelContextProtocolServer.ThinQConnect.Configuration;

internal sealed class ThinQConnectOptions
{
    internal const string DefaultApiKey = "v6GFvkweNo7DK7yD3ylIZ9w52aKBU0eJ7wLXkSR3";
    internal const string DefaultServicePhase = "OP";
    internal static readonly string[] DefaultRouteServers =
    [
        "https://api-kic.lgthinq.com",
        "https://api-aic.lgthinq.com",
        "https://api-eic.lgthinq.com"
    ];

    private static readonly string GeneratedClientId = $"mcpserver-thinqconnect-{Guid.NewGuid():N}";

    internal ThinQConnectOptions(
        string country,
        string? personalAccessToken,
        string servicePhase,
        string apiKey,
        string clientId,
        string? baseUrl)
    {
        Country = country;
        PersonalAccessToken = personalAccessToken;
        ServicePhase = servicePhase;
        ApiKey = apiKey;
        ClientId = clientId;
        BaseUrl = baseUrl;
    }

    public string Country { get; }

    public string? PersonalAccessToken { get; }

    public string ServicePhase { get; }

    public string ApiKey { get; }

    public string ClientId { get; }

    public string? BaseUrl { get; }

    internal static ThinQConnectOptions FromConfiguration(IConfiguration configuration)
    {
        var country = GetRequired(configuration, "country", "Country", "THINQ_COUNTRY");
        var pat = GetOptional(configuration, "pat", "Pat", "PAT", "THINQ_PAT");
        var servicePhase = GetOptional(configuration, "servicePhase", "ServicePhase", "THINQ_SERVICE_PHASE") ?? DefaultServicePhase;
        var apiKey = GetOptional(configuration, "apiKey", "ApiKey", "THINQ_API_KEY") ?? DefaultApiKey;
        var clientId = GetOptional(configuration, "clientId", "ClientId", "THINQ_CLIENT_ID") ?? GeneratedClientId;
        var baseUrl = GetOptional(configuration, "baseUrl", "BaseUrl", "THINQ_BASE_URL");

        return new ThinQConnectOptions(country, pat, servicePhase, apiKey, clientId, baseUrl);
    }

    internal void EnsurePersonalAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(PersonalAccessToken))
        {
            return;
        }

        throw new InvalidOperationException("A ThinQ Personal Access Token is required for this operation. Set THINQ_PAT or pass --pat <token>.");
    }

    private static string GetRequired(IConfiguration configuration, params string[] keys)
    {
        return GetOptional(configuration, keys)
               ?? throw new InvalidOperationException($"Missing required ThinQ configuration. Set one of: {string.Join(", ", keys)}.");
    }

    private static string? GetOptional(IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
