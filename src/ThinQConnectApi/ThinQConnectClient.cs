using System.ComponentModel;
using System.Net.Http.Json;
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

    private readonly Lazy<string> servicePhase = new(() => configuration.GetOptional("servicephase", "THINQ_SERVICEPHASE") ?? DefaultServicePhase);

    private readonly HttpClient httpClient = httpClientFactory.CreateClient(ThinQConnectHttpClientName);

    public async Task<ThinQRoute> GetRoute(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/route");
        request.Headers.TryAddWithoutValidation("x-service-phase", servicePhase.Value);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ThinQRoute>(cancellationToken: cancellationToken))!;
    }

    public Task<Devices> GetDevices(CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<Devices>("/devices", cancellationToken)!;
    }

    public Task<string> GetDeviceProfile(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetStringAsync($"/devices/{deviceId}/profile", cancellationToken);
    }

    public Task<string> GetDeviceState(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return httpClient.GetStringAsync($"/devices/{deviceId}/state", cancellationToken);
    }

    //public Task<string> ControlDevice(
    //    [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
    //    [Description("A JSON object string containing the exact control payload to send to ThinQ.")] string payloadJson,
    //    [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false, CancellationToken cancellationToken = default)
    //{
    //    return thinQConnectClient.ControlDeviceAsync(deviceId, payloadJson, conditionalControl);
    //}
}