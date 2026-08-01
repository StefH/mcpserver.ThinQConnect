using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ThinQConnectApi.Models.Devices;
using ThinQConnectApi.Models.Route;

namespace ThinQConnectApi;

public class ThinQConnectClient(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IThinQConnectClient
{
    public const string ThinQConnectHttpClientName = nameof(ThinQConnectClient);

    // OP — production/operational environment
    // QA — QA / test environment
    // DEV — development environment
    private const string DefaultServicePhase = "OP";

    private readonly string servicePhase = configuration.GetOptional("servicephase", "THINQ_SERVICEPHASE") ?? DefaultServicePhase;
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ThinQConnectHttpClientName);

    public async Task<ThinQRoute> GetRouteAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/route");
        request.Headers.TryAddWithoutValidation("x-service-phase", servicePhase);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ThinQRoute>(cancellationToken: cancellationToken))!;
    }

    public Task<Devices> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<Devices>("/devices", cancellationToken)!;
    }

    public Task<string> GetDevicesRawAsync(CancellationToken cancellationToken = default)
    {
        return httpClient.GetStringAsync("/devices", cancellationToken);
    }

    public Task<JsonElement> GetDeviceProfileAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<JsonElement>($"/devices/{deviceId}/profile", cancellationToken);
    }

    public Task<string> GetDeviceProfileRawAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetStringAsync($"/devices/{deviceId}/profile", cancellationToken);
    }

    public Task<JsonElement> GetDeviceStateAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<JsonElement>($"/devices/{deviceId}/state", cancellationToken);
    }

    public Task<string> GetDeviceStateRawAsync(
    [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetStringAsync($"/devices/{deviceId}/state", cancellationToken);
    }

    public async Task<JsonElement> ControlDeviceAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] JsonElement payload,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/devices/{deviceId}/control")
        {
            Content = JsonContent.Create(payload)
        };

        if (conditionalControl)
        {
            request.Headers.TryAddWithoutValidation("x-conditional-control", "true");
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
    }

    public async Task<string> ControlDeviceRawAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] string payload,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/devices/{deviceId}/control")
        {
            Content = JsonContent.Create(payload)
        };

        if (conditionalControl)
        {
            request.Headers.TryAddWithoutValidation("x-conditional-control", "true");
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}