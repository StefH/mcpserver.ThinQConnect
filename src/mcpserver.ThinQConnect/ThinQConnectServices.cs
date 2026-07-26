using ModelContextProtocolServer.ThinQConnect.Tools;
using ThinQConnectApi;

namespace ModelContextProtocolServer.ThinQConnect;

public static class ThinQConnectServices
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        ThinQConnectClientServices.Register(services, configuration);

        services.AddSingleton<ThinQConnectTools>();
    }
}