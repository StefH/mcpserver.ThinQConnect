using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocolServer.Hybrid;

await HybridServer.RunAsync(services =>
{
    services.AddHttpClient("ThinQConnect");
}, args);
