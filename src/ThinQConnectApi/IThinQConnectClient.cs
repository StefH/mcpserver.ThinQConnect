using System.Text.Json;
using ThinQConnectApi.Models.Devices;
using ThinQConnectApi.Models.Route;

namespace ThinQConnectApi;

public interface IThinQConnectClient
{
    /// <summary>
    /// Call the ThinQ Device API to return a device profile.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<JsonElement> GetDeviceProfileAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to return a device profile as raw JSON.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<string> GetDeviceProfileRawAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to return the registered devices.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<Devices> GetDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to return the registered devices as raw JSON.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<string> GetDevicesRawAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to return the current device state.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<JsonElement> GetDeviceStateAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to return the current device state as raw JSON.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<string> GetDeviceStateRawAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Route API and return the resolved backend endpoints.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<ThinQRoute> GetRouteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to send a control payload to a device.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="payload">A JSON object containing the exact control payload to send to ThinQ Device API..</param>
    /// <param name="conditionalControl">When true, sends the x-conditional-control header so the command only executes in controllable states.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<JsonElement> ControlDeviceAsync(
        string deviceId,
        JsonElement payload,
        bool conditionalControl = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the ThinQ Device API to send a control payload to a device as raw JSON.
    /// </summary>
    /// <param name="deviceId">The ThinQ device identifier returned by GetDevices.</param>
    /// <param name="payloadJson">A JSON object string containing the exact control payload to send to ThinQ Device API..</param>
    /// <param name="conditionalControl">When true, sends the x-conditional-control header so the command only executes in controllable states.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<string> ControlDeviceRawAsync(
        string deviceId,
        string payloadJson,
        bool conditionalControl = false,
        CancellationToken cancellationToken = default);
}
