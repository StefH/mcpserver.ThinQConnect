using ThinQConnectApi.Models;

namespace ThinQConnectApi.Models.Route;

public class ThinQRoute : BaseModel
{
    public required RouteServers Response { get; set; }
}