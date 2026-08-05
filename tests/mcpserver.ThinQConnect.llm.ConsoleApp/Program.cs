using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

AzureOpenAIClient azureClient = new(
    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_URL2")!),
    new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY2")!));

IChatClient chatClient = azureClient.GetChatClient("gpt-5")
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

foreach (var (name, client, processId) in new[] { await CreateStdioClientAsync() }) // , await CreateHttpClientAsync()
{
    Console.WriteLine("\r\nMcpClient: " + name);

    var mcpTools = await client.ListToolsAsync();

    var chatOptions = new ChatOptions
    {
        Instructions = client.ServerInstructions,
        Tools = [.. mcpTools] // MCP tools already implement AITool
    };

    var response = await chatClient.GetResponseAsync("Which LG Devices are registered? Only return the name and model.", chatOptions);
    Console.WriteLine(response.Text);

    response = await chatClient.GetResponseAsync("What are the current temperatures in C of the LG Devices? And also return the average temperature.", chatOptions);
    Console.WriteLine(response.Text);

    await client.DisposeAsync();

    if (processId > 0)
    {
        try
        {
            Process.GetProcessById(processId).Kill();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to kill ThinQConnect MCP HTTP server process (ID: {processId}): {ex.Message}");
        }
    }
}

return;

static async Task<(string Name, McpClient Client, int ProcessId)> CreateStdioClientAsync()
{
    string cd = Directory.GetCurrentDirectory();

    var client = await McpClient.CreateAsync(new StdioClientTransport(new()
    {
        Command = "dotnet run --project",
        Arguments = [$"{cd}/../../../../../src/mcpserver.ThinQConnect/mcpserver.ThinQConnect.csproj"],
        Name = "ThinQConnect"
    }));

    return ("Stdio", client, -1);
}

static async Task<(string Name, McpClient Client, int ProcessId)> CreateHttpClientAsync()
{
    var processId = StartThinQConnectMcpHttpServer();

    await Task.Delay(2000); // Wait for the server to start

    var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri("https://localhost:60433"),
        Name = "ThinQConnect"
    }));

    return ("Http", client, processId);
}

static int StartThinQConnectMcpHttpServer()
{
    string cd = Directory.GetCurrentDirectory();
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {cd}/../../../../../src/mcpserver.ThinQConnect/mcpserver.ThinQConnect.csproj --sse",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine($"[ThinQConnect MCP HTTP Server] {e.Data}"); };
    process.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.Error.WriteLine($"[ThinQConnect MCP HTTP Server] {e.Data}"); };
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    return process.Id;
}