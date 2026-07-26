namespace ThinQConnectApi.Models.Route;

public class RouteServers
{
    public required string ApiServer { get; set; }

    public required string MqttServer { get; set; }

    public required string WebSocketServer { get; set; }

    public required HomeyCloud HomeyCloud { get; set; }
}