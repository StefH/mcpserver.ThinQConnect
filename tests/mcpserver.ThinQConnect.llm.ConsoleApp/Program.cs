using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Command = "dotnet run --project", // or "dotnet", "python", whatever launches your server
    Arguments = [@"C:\dev\GitHub\mcpserver.ThinQConnect\src\mcpserver.ThinQConnect\mcpserver.ThinQConnect.csproj"],
    Name = "ThinQConnect"
}));

var mcpTools = await mcpClient.ListToolsAsync();

AzureOpenAIClient azureClient = new(
    new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_URL2")!),
    new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY2")!));

IChatClient chatClient = azureClient.GetChatClient("gpt-5")
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

var chatOptions = new ChatOptions
{
    Instructions = "You are a helpful assistant that can answer questions about LG Devices. Use the tools provided to fetch information when necessary.",
    Tools = [.. mcpTools] // MCP tools already implement AITool
};


var response = await chatClient.GetResponseAsync("Which LG Devices are registered? Only return the name and model.", chatOptions);
Console.WriteLine(response.Text);

response = await chatClient.GetResponseAsync("What are the current temperatures in C of the LG Devices? And also return the average temperature.", chatOptions);
Console.WriteLine(response.Text);