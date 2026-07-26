using ThinQConnectApi.Models;

namespace ThinQConnectApi.Models.Devices;

public class Devices : BaseModel
{
    public DevicesResponse[] Response { get; set; } = [];
}