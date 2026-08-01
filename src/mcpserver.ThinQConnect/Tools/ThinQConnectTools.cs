using System.ComponentModel;
using System.Text.Json;
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
        return await thinQConnectClient.GetRouteAsync(cancellationToken);
    }

    [McpServerTool(ReadOnly = true, UseStructuredContent = true)]
    [Description("Call the ThinQ Device API to return the registered devices.")]
    public Task<Devices> GetDevices(CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDevicesAsync(cancellationToken);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return a device profile.")]
    public Task<JsonElement> GetDeviceProfile(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDeviceProfileAsync(deviceId, cancellationToken);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return mutiple device profiles.")]
    public Task<JsonElement[]> GetDeviceProfiles(
        [Description("The ThinQ device identifiers returned by GetDevices.")] string[] deviceIds, CancellationToken cancellationToken = default)
    {
        var tasks = deviceIds.Select(deviceId => thinQConnectClient.GetDeviceProfileAsync(deviceId, cancellationToken));
        return Task.WhenAll(tasks);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return the current device state.")]
    public Task<JsonElement> GetDeviceState(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.GetDeviceStateAsync(deviceId, cancellationToken);
    }

    [McpServerTool(ReadOnly = true)]
    [Description("Call the ThinQ Device API to return the current device states for multiple devices.")]
    public Task<JsonElement[]> GetDeviceStates(
        [Description("The ThinQ device identifiers returned by GetDevices.")] string[] deviceIds, CancellationToken cancellationToken = default)
    {
        var tasks = deviceIds.Select(deviceId => thinQConnectClient.GetDeviceStateAsync(deviceId, cancellationToken));
        return Task.WhenAll(tasks);
    }

    [McpServerTool]
    [Description("Call the ThinQ Device API to send a control payload to a device. The payload value must be valid JSON.")]
    public Task<JsonElement> ControlDevice(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] JsonElement payload,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false,
        CancellationToken cancellationToken = default)
    {
        return thinQConnectClient.ControlDeviceAsync(deviceId, payload, conditionalControl, cancellationToken);
    }

    [McpServerTool]
    [Description("Call the ThinQ Device API to send a control payload to multiple devices. The payload value must be valid JSON.")]
    public Task<JsonElement[]> ControlDevices(
        [Description("The ThinQ device identifiers returned by GetDevices.")] string[] deviceIds,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] JsonElement payload,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false,
        CancellationToken cancellationToken = default)
    {
        var tasks = deviceIds.Select(deviceId => thinQConnectClient.ControlDeviceAsync(deviceId, payload, conditionalControl, cancellationToken));
        return Task.WhenAll(tasks);
    }
}