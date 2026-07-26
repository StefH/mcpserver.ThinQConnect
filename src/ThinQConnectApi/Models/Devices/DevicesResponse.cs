namespace ThinQConnectApi.Models.Devices;

public class DevicesResponse
{
    public required string DeviceId { get; set; }

    public required DeviceInfo DeviceInfo { get; set; }
}
