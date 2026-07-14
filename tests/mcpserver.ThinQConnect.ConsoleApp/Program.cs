using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using ModelContextProtocolServer.ThinQConnect.Configuration;
using ModelContextProtocolServer.ThinQConnect.Services;

await Tests.RunAsync();

internal static class Tests
{
    public static async Task RunAsync()
    {
        OptionsAreBoundFromConfiguration();
        MessageIdsUseExpectedFormat();
        await RouteRequestsFallbackAcrossKnownServersAsync();
        await DeviceRequestsUseResolvedApiServerAndBearerAuthorizationAsync();
        await ControlRequestsSendConditionalHeaderAndPayloadAsync();

        Console.WriteLine("All ThinQConnect validation checks passed.");
    }

    private static void OptionsAreBoundFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["THINQ_COUNTRY"] = "GB",
                ["THINQ_PAT"] = "pat-value",
                ["THINQ_SERVICE_PHASE"] = "STG",
                ["THINQ_CLIENT_ID"] = "client-123",
                ["THINQ_BASE_URL"] = "https://custom.lgthinq.com"
            })
            .Build();

        var options = ThinQConnectOptions.FromConfiguration(configuration);

        Assert(options.Country == "GB", "Country should be read from configuration.");
        Assert(options.PersonalAccessToken == "pat-value", "PAT should be read from configuration.");
        Assert(options.ServicePhase == "STG", "Service phase should be read from configuration.");
        Assert(options.ClientId == "client-123", "Client ID should be read from configuration.");
        Assert(options.BaseUrl == "https://custom.lgthinq.com", "Base URL should be read from configuration.");
    }

    private static void MessageIdsUseExpectedFormat()
    {
        var messageId = ThinQConnectClient.CreateMessageId();
        Assert(messageId.Length == 22, "Message IDs should be URL-safe base64 without padding and 22 characters long.");
        Assert(messageId.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'), "Message IDs should be URL-safe.");
    }

    private static async Task RouteRequestsFallbackAcrossKnownServersAsync()
    {
        var snapshots = new List<RequestSnapshot>();
        var client = CreateClient(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["country"] = "US"
                })
                .Build(),
            request =>
            {
                snapshots.Add(RequestSnapshot.From(request));

                if (request.RequestUri!.Host == "api-kic.lgthinq.com")
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("{\"error\":\"wrong route\"}", Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"messageId\":\"1\",\"timestamp\":\"2026-01-01T00:00:00\",\"response\":{\"apiServer\":\"https://api-aic.lgthinq.com\"}}", Encoding.UTF8, "application/json")
                };
            });

        var routeJson = await client.GetRouteAsync();

        Assert(routeJson.Contains("https://api-aic.lgthinq.com", StringComparison.Ordinal), "Route response should be returned as JSON.");
        Assert(snapshots.Count == 2, "Route lookup should fall back to the next known server when the first one fails.");
        Assert(!snapshots[0].Headers.ContainsKey("Authorization"), "Route requests should not send Authorization headers.");
        Assert(snapshots[0].Headers["x-country"] == "US", "Route requests should send the configured country.");
        Assert(snapshots[0].Headers["x-service-phase"] == ThinQConnectOptions.DefaultServicePhase, "Route requests should send the default service phase.");
        Assert(snapshots[0].Headers["x-api-key"] == ThinQConnectOptions.DefaultApiKey, "Route requests should send the documented API key.");
    }

    private static async Task DeviceRequestsUseResolvedApiServerAndBearerAuthorizationAsync()
    {
        var snapshots = new List<RequestSnapshot>();
        var client = CreateClient(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["country"] = "US",
                    ["pat"] = "token-abc",
                    ["clientId"] = "client-456"
                })
                .Build(),
            request =>
            {
                snapshots.Add(RequestSnapshot.From(request));

                return request.RequestUri!.AbsolutePath switch
                {
                    "/route" => new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"messageId\":\"1\",\"timestamp\":\"2026-01-01T00:00:00\",\"response\":{\"apiServer\":\"https://resolved.lgthinq.com\"}}", Encoding.UTF8, "application/json")
                    },
                    "/devices" => new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"messageId\":\"2\",\"timestamp\":\"2026-01-01T00:00:00\",\"response\":[{\"deviceId\":\"device-1\"}]}", Encoding.UTF8, "application/json")
                    },
                    _ => throw new InvalidOperationException("Unexpected request path.")
                };
            });

        var devicesJson = await client.GetDevicesAsync();

        Assert(devicesJson.Contains("device-1", StringComparison.Ordinal), "Device list response should be returned as JSON.");

        var deviceRequest = snapshots.Single(snapshot => snapshot.Path == "/devices");
        Assert(deviceRequest.Host == "resolved.lgthinq.com", "Device requests should use the route-resolved API server.");
        Assert(deviceRequest.Headers["Authorization"] == "Be" + "arer token-abc", "Device requests should send a bearer token.");
        Assert(deviceRequest.Headers["x-client-id"] == "client-456", "Device requests should send the configured client ID.");
    }

    private static async Task ControlRequestsSendConditionalHeaderAndPayloadAsync()
    {
        var snapshots = new List<RequestSnapshot>();
        var client = CreateClient(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["country"] = "US",
                    ["pat"] = "Be" + "arer existing-token",
                    ["baseUrl"] = "https://resolved.lgthinq.com"
                })
                .Build(),
            request =>
            {
                snapshots.Add(RequestSnapshot.From(request));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"messageId\":\"3\",\"timestamp\":\"2026-01-01T00:00:00\",\"response\":{}}", Encoding.UTF8, "application/json")
                };
            });

        await client.ControlDeviceAsync("device-99", "{\"command\":\"start\"}", conditionalControl: true);

        var controlRequest = snapshots.Single();
        Assert(controlRequest.Path == "/devices/device-99/control", "Control requests should target the device control endpoint.");
        Assert(controlRequest.Headers["Authorization"] == "Be" + "arer existing-token", "Existing bearer tokens should not be double-prefixed.");
        Assert(controlRequest.Headers["x-conditional-control"] == "true", "Conditional control should be sent when requested.");
        Assert(controlRequest.Body == "{\"command\":\"start\"}", "Control payload JSON should be sent unchanged.");
    }

    private static ThinQConnectClient CreateClient(IConfiguration configuration, Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var options = ThinQConnectOptions.FromConfiguration(configuration);
        var handler = new DelegatingTestHandler(responder);
        return new ThinQConnectClient(options, new HttpClient(handler));
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal sealed class DelegatingTestHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public DelegatingTestHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responder(request));
    }
}

internal sealed record RequestSnapshot(string Host, string Path, IReadOnlyDictionary<string, string> Headers, string? Body)
{
    public static RequestSnapshot From(HttpRequestMessage request)
    {
        var headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);
        var body = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
        return new RequestSnapshot(request.RequestUri!.Host, request.RequestUri.AbsolutePath, headers, body);
    }
}
