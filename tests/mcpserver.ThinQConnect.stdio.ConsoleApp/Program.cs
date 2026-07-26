using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.ThinQConnect;
using ModelContextProtocolServer.ThinQConnect.Tools;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

ThinQConnectServices.Register(services, configuration);
var serviceProvider = services.BuildServiceProvider();

var connect = serviceProvider.GetRequiredService<ThinQConnectTools>();

var route = await connect.GetRoute();
Console.WriteLine("GetRoute: {0}", ToJson(route));

var devices = await connect.GetDevices();
Console.WriteLine("GetDevices: {0}", ToJson(devices));

static string ToJson(object value)
{
    return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
}