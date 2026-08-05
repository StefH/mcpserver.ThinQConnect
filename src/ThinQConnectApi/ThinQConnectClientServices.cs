using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ThinQConnectApi;

public static class ThinQConnectClientServices
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddHttpClient(ThinQConnectClient.ThinQConnectHttpClientName, httpClient =>
        {
            const string defaultApiKey = "v6GFvkweNo7DK7yD3ylIZ9w52aKBU0eJ7wLXkSR3";
            const string defaultCountry = "NL";
            const string generatedClientId = "mcpserver-thinqconnect";
            const string defaultBaseUrl = "https://api-eic.lgthinq.com";

            var pat = configuration.GetOptional("pat", "THINQ_PAT") ?? throw new ArgumentException("A ThinQ Personal Access Token is required for this operation. Set THINQ_PAT or pass --pat <token>.");
            var country = configuration.GetOptional("country", "THINQ_COUNTRY") ?? defaultCountry;
            var baseUrl = configuration.GetOptional("baseurl", "THINQ_BASEURL") ?? defaultBaseUrl;

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", defaultApiKey);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-message-id", CreateMessageId());
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-country", country);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-client-id", generatedClientId);

            httpClient.BaseAddress = new Uri(baseUrl);
        })
        .AddPolicyHandler(_ =>
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == (HttpStatusCode)429)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        });

        services.AddTransient<IThinQConnectClient, ThinQConnectClient>();
    }

    internal static string? GetOptional(this IConfiguration configuration, params string[] keys)
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

    // Generated with the url-safe-base64-no-padding (UUID Version 4) method. The length is 22 characters.
    static string CreateMessageId()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}