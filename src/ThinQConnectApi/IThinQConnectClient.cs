using System.ComponentModel;
using ThinQConnectApi.Models.Devices;
using ThinQConnectApi.Models.Route;

namespace ThinQConnectApi;

public interface IThinQConnectClient
{
    Task<string> GetDeviceProfile([Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default);

    Task<Devices> GetDevices(CancellationToken cancellationToken = default);

    Task<string> GetDeviceState([Description("The ThinQ device identifier returned by GetDevices.")] string deviceId, CancellationToken cancellationToken = default);

    Task<ThinQRoute> GetRoute(CancellationToken cancellationToken = default);
}