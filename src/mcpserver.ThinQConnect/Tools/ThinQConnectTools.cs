using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using ModelContextProtocolServer.ThinQConnect.Configuration;
using ModelContextProtocolServer.ThinQConnect.Services;

namespace ModelContextProtocolServer.ThinQConnect.Tools;

[McpServerToolType]
public static class ThinQConnectTools
{
    [McpServerTool, Description("Call the ThinQ Route API and return the resolved backend endpoints as formatted JSON.")]
    public static Task<string> GetRoute(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        return CreateClient(configuration, httpClientFactory).GetRouteAsync();
    }

    [McpServerTool, Description("Call the ThinQ Device API to return the registered device list as formatted JSON.")]
    public static Task<string> GetDevices(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        return CreateClient(configuration, httpClientFactory).GetDevicesAsync();
    }

    [McpServerTool, Description("Call the ThinQ Device API to return a device profile as formatted JSON.")]
    public static Task<string> GetDeviceProfile(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId)
    {
        return CreateClient(configuration, httpClientFactory).GetDeviceProfileAsync(deviceId);
    }

    [McpServerTool, Description("Call the ThinQ Device API to return the current device state as formatted JSON.")]
    public static Task<string> GetDeviceState(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId)
    {
        return CreateClient(configuration, httpClientFactory).GetDeviceStateAsync(deviceId);
    }

    [McpServerTool, Description("Call the ThinQ Device API to send a control payload to a device. The payloadJson value must be valid JSON.")]
    public static Task<string> ControlDevice(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] string payloadJson,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false)
    {
        return CreateClient(configuration, httpClientFactory).ControlDeviceAsync(deviceId, payloadJson, conditionalControl);
    }

    private static ThinQConnectClient CreateClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        return new ThinQConnectClient(ThinQConnectOptions.FromConfiguration(configuration), httpClientFactory);
    }
}
