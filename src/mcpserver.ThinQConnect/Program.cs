using ModelContextProtocolServer.Hybrid;
using ModelContextProtocolServer.ThinQConnect;

await HybridServer.RunAsync(ThinQConnectServices.Register, args);