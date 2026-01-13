using CSharpMcpDemo.Prompts;
using CSharpMcpDemo.Resources;
using CSharpMcpDemo.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;

namespace CSharpMcpDemo;

/// <summary>
/// Entry point for the C# MCP Demo Server.
/// This demonstrates advanced MCP features for C# developers including:
/// - Tools: Executable operations
/// - Prompts: Reusable prompt templates
/// - Resources: Structured data access
/// </summary>
class Program
{
#pragma warning disable CS8892
    static async Task Main(string[] args)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging to stderr (MCP requirement for stdio)
            builder.Logging.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Trace;
            });
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Configure MCP server with stdio transport and register tools/prompts/resources
            var mcpBuilder = builder.Services.AddMcpServer();
            mcpBuilder.WithStdioServerTransport();
            
            // Register MCP server context for tools to access notifications and sampling
            builder.Services.AddSingleton<IMcpServerContext, McpServerContext>();
            
            mcpBuilder.WithTools<FileAnalysisTools>();
            mcpBuilder.WithPrompts<FileAnalysisPrompts>();
            mcpBuilder.WithResources<FileAnalysisResources>();

            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
