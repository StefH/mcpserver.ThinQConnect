using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocolServer.ThinQConnect.Configuration;

namespace ModelContextProtocolServer.ThinQConnect.Services;

internal sealed class ThinQConnectClient
{
    private readonly HttpClient _httpClient;
    private readonly ThinQConnectOptions _options;

    internal ThinQConnectClient(ThinQConnectOptions options, HttpClient? httpClient = null)
    {
        _options = options;
        _httpClient = httpClient ?? new HttpClient();
    }

    public Task<string> GetRouteAsync(CancellationToken cancellationToken = default)
    {
        return GetRouteAsyncCore(cancellationToken);
    }

    public async Task<string> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        _options.EnsurePersonalAccessToken();
        var baseUri = await ResolveApiServerAsync(cancellationToken).ConfigureAwait(false);
        return await SendAsync(HttpMethod.Get, new Uri(baseUri, "/devices"), requiresAuthorization: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetDeviceProfileAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        _options.EnsurePersonalAccessToken();
        var baseUri = await ResolveApiServerAsync(cancellationToken).ConfigureAwait(false);
        return await SendAsync(HttpMethod.Get, new Uri(baseUri, $"/devices/{Uri.EscapeDataString(deviceId)}/profile"), requiresAuthorization: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetDeviceStateAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        _options.EnsurePersonalAccessToken();
        var baseUri = await ResolveApiServerAsync(cancellationToken).ConfigureAwait(false);
        return await SendAsync(HttpMethod.Get, new Uri(baseUri, $"/devices/{Uri.EscapeDataString(deviceId)}/state"), requiresAuthorization: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ControlDeviceAsync(string deviceId, string payloadJson, bool conditionalControl = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);
        _options.EnsurePersonalAccessToken();
        if (JsonNode.Parse(payloadJson) is not JsonObject)
        {
            throw new InvalidOperationException("Control payload JSON must be a JSON object.");
        }

        var baseUri = await ResolveApiServerAsync(cancellationToken).ConfigureAwait(false);
        return await SendAsync(
            HttpMethod.Post,
            new Uri(baseUri, $"/devices/{Uri.EscapeDataString(deviceId)}/control"),
            requiresAuthorization: true,
            payloadJson: payloadJson,
            conditionalControl: conditionalControl,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    internal static string CreateMessageId()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    internal static string NormalizeBearerToken(string personalAccessToken)
    {
        var normalizedToken = personalAccessToken.Trim();
        return normalizedToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? normalizedToken
            : "Bearer " + normalizedToken;
    }

    private async Task<Uri> ResolveApiServerAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return CreateBaseUri(_options.BaseUrl);
        }

        var routeJson = await GetRouteAsyncCore(cancellationToken).ConfigureAwait(false);
        var route = JsonNode.Parse(routeJson);
        var apiServer = route?["response"]?["apiServer"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(apiServer))
        {
            throw new InvalidOperationException("The Route API response did not contain response.apiServer.");
        }

        return CreateBaseUri(apiServer);
    }

    private async Task<string> GetRouteAsyncCore(CancellationToken cancellationToken)
    {
        var candidates = BuildRouteCandidates();
        var failures = new List<string>();

        foreach (var candidate in candidates)
        {
            try
            {
                return await SendAsync(HttpMethod.Get, new Uri(CreateBaseUri(candidate), "/route"), requiresAuthorization: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {
                failures.Add($"{candidate}: {exception.Message}");
            }
        }

        throw new HttpRequestException($"Unable to resolve ThinQ route information. Attempts: {string.Join(" | ", failures)}");
    }

    private async Task<string> SendAsync(
        HttpMethod method,
        Uri requestUri,
        bool requiresAuthorization,
        string? payloadJson = null,
        bool? conditionalControl = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("x-message-id", CreateMessageId());
        request.Headers.TryAddWithoutValidation("x-country", _options.Country);
        request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);

        if (requestUri.AbsolutePath.Equals("/route", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation("x-service-phase", _options.ServicePhase);
        }
        else
        {
            request.Headers.TryAddWithoutValidation("x-client-id", _options.ClientId);
        }

        if (requiresAuthorization)
        {
            request.Headers.TryAddWithoutValidation("Authorization", NormalizeBearerToken(_options.PersonalAccessToken!));
        }

        if (conditionalControl.HasValue)
        {
            request.Headers.TryAddWithoutValidation("x-conditional-control", conditionalControl.Value ? "true" : "false");
        }

        if (payloadJson is not null)
        {
            request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"ThinQ request to '{requestUri}' failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}");
        }

        return TryFormatJson(body);
    }

    private IEnumerable<string> BuildRouteCandidates()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl) && seen.Add(_options.BaseUrl))
        {
            yield return _options.BaseUrl;
        }

        foreach (var routeServer in ThinQConnectOptions.DefaultRouteServers)
        {
            if (seen.Add(routeServer))
            {
                yield return routeServer;
            }
        }
    }

    private static Uri CreateBaseUri(string value)
    {
        var normalized = value.EndsWith('/') ? value : $"{value}/";
        return new Uri(normalized, UriKind.Absolute);
    }

    private static string TryFormatJson(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return body;
        }
    }
}
