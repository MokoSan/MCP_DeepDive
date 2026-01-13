using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSharpMcpDemo;

/// <summary>
/// Provides access to MCP server context for tools to send notifications and sampling requests.
/// This interface allows tools to interact with the MCP protocol beyond simple request/response.
/// </summary>
public interface IMcpServerContext
{
    /// <summary>
    /// Sends a progress notification to the client.
    /// Used for long-running operations to provide real-time progress updates.
    /// </summary>
    void SendProgressNotification(string progressToken, int progress, int total);
    
    /// <summary>
    /// Sends a sampling request to the client's LLM.
    /// Allows the server to delegate AI inference to the client.
    /// </summary>
    Task<JsonNode?> SendSamplingRequestAsync(object request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a log message notification to the client.
    /// </summary>
    void SendLogNotification(LoggingLevel level, string message, string? logger = null);
}

/// <summary>
/// Implementation of MCP server context using JSON-RPC over the configured transport.
/// This is injected into tools via dependency injection.
/// </summary>
public class McpServerContext : IMcpServerContext
{
    private readonly ILogger<McpServerContext> _logger;
    
    public McpServerContext(ILogger<McpServerContext> logger)
    {
        _logger = logger;
    }
    
    public void SendProgressNotification(string progressToken, int progress, int total)
    {
        // For now, use logging - the SDK handles converting logs to MCP notifications
        _logger.LogInformation("[MCP Progress] Token: {Token}, Progress: {Progress}/{Total}", 
            progressToken, progress, total);
    }
    
    public async Task<JsonNode?> SendSamplingRequestAsync(object request, CancellationToken cancellationToken = default)
    {
        // Sampling requires direct access to the transport layer
        // which isn't exposed through tools in the current SDK architecture
        // This would need to be implemented at the server level
        await Task.CompletedTask;
        throw new NotSupportedException(
            "Sampling requests require server-level implementation. " +
            "The current SDK does not expose sampling capability to tools directly.");
    }
    
    public void SendLogNotification(LoggingLevel level, string message, string? logger = null)
    {
        switch (level)
        {
            case LoggingLevel.Debug:
                _logger.LogDebug("{Message}", message);
                break;
            case LoggingLevel.Info:
                _logger.LogInformation("{Message}", message);
                break;
            case LoggingLevel.Warning:
                _logger.LogWarning("{Message}", message);
                break;
            case LoggingLevel.Error:
                _logger.LogError("{Message}", message);
                break;
        }
    }
}
