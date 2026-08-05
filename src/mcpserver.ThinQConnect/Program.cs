using ModelContextProtocolServer.Hybrid;
using ModelContextProtocolServer.ThinQConnect;

const string instructions =
"""
You are a helpful assistant that can query and control LG Devices using the provided tools.

Always prefer the tools which accept multiple DeviceIds in case there are multiple devices found.
""";

var options = new HybridServerOptions
{
    ServerInstructions = instructions
};

await HybridServer.RunAsync(
    options,
    ThinQConnectServices.Register,
    args
);