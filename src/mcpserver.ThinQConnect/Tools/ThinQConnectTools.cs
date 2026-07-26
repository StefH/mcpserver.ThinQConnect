using System.ComponentModel;
using ModelContextProtocol.Server;
using ThinQConnectApi;
using ThinQConnectApi.Models.Devices;
using ThinQConnectApi.Models.Route;

namespace ModelContextProtocolServer.ThinQConnect.Tools;

[McpServerToolType]
public class ThinQConnectTools(IThinQConnectClient thinQConnectClient)
{
    [McpServerTool(ReadOnly = true, UseStructuredContent = true)]
    [Description("Call the ThinQ Route API and return the resolved backend endpoints.")]
    public async Task<ThinQRoute> GetRoute(CancellationToken cancellationToken = default)
    {
        return await thinQConnectClient.GetRoute(cancellationToken);
    }

    [McpServerTool(ReadOnly = true, UseStructuredContent = true)]
    [Description("Call the ThinQ Device API to return the registered devices.")]
    public Task<Devices> GetDevices(CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDevices(cancellationToken);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return a device profile.")]
    public Task<string> GetDeviceProfile(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDeviceProfile(deviceId, cancellationToken);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return the current device state.")]
    public Task<string> GetDeviceState(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDeviceState(deviceId, cancellationToken);
    }

    //[McpServerTool]
    //[Description("Call the ThinQ Device API to send a control payload to a device. The payloadJson value must be valid JSON.")]
    //public Task<string> ControlDevice(
    //    [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
    //    [Description("A JSON object string containing the exact control payload to send to ThinQ.")] string payloadJson,
    //    [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false, CancellationToken cancellationToken = default)
    //{
    //    return thinQConnectClient.ControlDeviceAsync(deviceId, payloadJson, conditionalControl);
    //}
}
