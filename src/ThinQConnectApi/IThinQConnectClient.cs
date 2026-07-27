using System.ComponentModel;
using ThinQConnectApi.Models.Devices;
using ThinQConnectApi.Models.Route;

namespace ThinQConnectApi;

public interface IThinQConnectClient
{
    Task<string> GetDeviceProfileAsync([Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default);

    Task<Devices> GetDevicesAsync(CancellationToken cancellationToken = default);

    Task<string> GetDeviceStateAsync([Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default);

    Task<ThinQRoute> GetRouteAsync(CancellationToken cancellationToken = default);

    Task<string> ControlDeviceAsync(
        [Description("The ThinQ device identifier returned by GetDevices.")] string deviceId,
        [Description("A JSON object string containing the exact control payload to send to ThinQ.")] string payloadJson,
        [Description("When true, sends the x-conditional-control header so the command only executes in controllable states.")] bool conditionalControl = false,
        CancellationToken cancellationToken = default);
}