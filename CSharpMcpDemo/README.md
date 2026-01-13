# C# MCP Demo - Advanced Topics Showcase

A comprehensive Model Context Protocol (MCP) server demonstration for C# developers, showcasing advanced MCP features through practical file system operations.

## Overview

This project demonstrates the following MCP advanced features:

- **🤖 Sampling**: Server requests AI analysis from the client's LLM
- **🔒 Roots**: Secure file system access with permission boundaries
- **📊 Progress Notifications**: Real-time updates for long-running operations
- **📝 Logging**: Structured log messages at multiple levels
- **💬 Message Types**: Proper use of Requests, Responses, and Notifications

## Architecture

```
CSharpMcpDemo/
├── Program.cs                      # Entry point with dependency injection
├── FileAnalysisMcpServer.cs        # Main MCP server implementation
├── CSharpMcpDemo.csproj            # Project file with MCP dependencies
└── appsettings.json                # Configuration
```

## Prerequisites

- .NET 10.0 SDK or later
- Basic understanding of async/await in C#
- An MCP-compatible client (e.g., Claude Desktop, VS Code with MCP extension)

## Installation

### As a .NET Tool

```bash
# Build and pack the tool
dotnet pack

# Install globally
dotnet tool install --global --add-source ./bin/Debug CSharpMcpDemo

# Run the tool
csharp-mcp-demo
```

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Run the server
dotnet run
```

## MCP Client Configuration

### Claude Desktop Configuration

Add to your Claude Desktop config (`%APPDATA%\Claude\claude_desktop_config.json` on Windows):

```json
{
  "mcpServers": {
    "csharp-demo": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Users\\musharm\\source\\repos\\MCP_Presentation\\CSharpMcpDemo\\CSharpMcpDemo.csproj"
      ]
    }
  }
}
```

Or if installed as a tool:

```json
{
  "mcpServers": {
    "csharp-demo": {
      "command": "csharp-mcp-demo"
    }
  }
}
```

## Features Demonstrated

### 1. 📊 Progress Notifications

**Tool**: `analyze_large_file`

Demonstrates how servers can send real-time progress updates for long-running operations.

```csharp
// Server sends progress notifications
await SendProgressAsync(progressToken, currentProgress, totalItems);

// Client receives updates and can show progress bars
```

**Try it**: Ask Claude to analyze a large text file and watch the progress updates.

**Example prompt**:
```
Use the analyze_large_file tool to analyze my README.md file
```

### 2. 🔒 Roots-Based Security

**Tool**: `read_file_secure`

Shows how MCP's roots capability provides a security boundary for file access.

```csharp
// Server validates file is within approved roots
if (!IsPathWithinRoots(filePath, _approvedRoots))
{
    throw new UnauthorizedAccessException();
}
```

**Try it**: Attempt to read files inside and outside approved directories.

**Example prompts**:
```
Read the contents of a file in my Documents folder (allowed)
Read C:\Windows\System32\config\sam (denied - outside roots)
```

### 3. 🤖 Sampling (AI-Assisted Analysis)

**Tool**: `ai_summarize_files`

Demonstrates how servers can request the client's LLM to perform analysis, offloading AI costs to the client.

```csharp
var samplingRequest = new SamplingCreateMessageRequest
{
    Messages = new[] { 
        new SamplingMessage { 
            Role = "user", 
            Content = new TextContent { Text = prompt } 
        } 
    },
    ModelPreferences = new ModelPreferences { 
        Hints = new[] { new ModelHint { Name = "claude-3-5-sonnet" } } 
    }
};
```

**Try it**: Ask to summarize multiple files using AI.

**Example prompt**:
```
Use ai_summarize_files to analyze all text files in my project directory
```

### 4. 📝 Structured Logging

**Tool**: `search_files`

Shows logging at different levels (Debug, Info, Warning, Error) for debugging and monitoring.

```csharp
await SendLogAsync(LoggingLevel.Info, "Starting search...");
await SendLogAsync(LoggingLevel.Debug, $"Processing file {fileName}");
await SendLogAsync(LoggingLevel.Warning, "File could not be read");
await SendLogAsync(LoggingLevel.Error, "Critical error occurred");
```

**Try it**: Search for text in files and observe the detailed logging.

**Example prompt**:
```
Search for the word "TODO" in all .cs files in my src directory
```

### 5. 🔄 Combined Features

**Tool**: `batch_process_files`

Combines progress tracking, logging, and batch processing for complex operations.

**Try it**: Process multiple files with real-time feedback.

**Example prompt**:
```
Use batch_process_files to count lines in all C# files in my project
```

## Available Tools

| Tool | Purpose | Features Demonstrated |
|------|---------|----------------------|
| `analyze_large_file` | Analyzes file content with statistics | Progress Notifications, Logging |
| `read_file_secure` | Reads files within approved roots | Roots Security |
| `ai_summarize_files` | AI-powered file summarization | Sampling |
| `search_files` | Searches files for patterns | Structured Logging |
| `batch_process_files` | Processes multiple files | All features combined |

## Key Concepts for C# Developers

### 1. Request vs Notification

**Request** (expects response):
```csharp
// Client sends request, server MUST respond
protected override async Task<ToolCallResult> HandleToolCallAsync(ToolCallRequest request)
{
    // Do work...
    return new ToolCallResult { Content = ... }; // Response required
}
```

**Notification** (fire-and-forget):
```csharp
// Server sends notification, no response expected
await _mcpServer.SendNotificationAsync("notifications/progress", progressData);
```

### 2. Progress Pattern

```csharp
// 1. Client includes progressToken in request
var request = new ToolCallRequest 
{ 
    Meta = new { ProgressToken = "unique-token" } 
};

// 2. Server sends periodic updates
for (int i = 0; i < total; i++)
{
    await SendProgressAsync(progressToken, i, total);
}

// 3. Server completes with final response
return CreateSuccessResult("Done!");
```

### 3. Roots Validation Pattern

```csharp
private bool IsPathWithinRoots(string filePath, List<Root> approvedRoots)
{
    var fileUri = new Uri(Path.GetFullPath(filePath));
    return approvedRoots.Any(root => 
        fileUri.AbsolutePath.StartsWith(
            new Uri(root.Uri).AbsolutePath, 
            StringComparison.OrdinalIgnoreCase));
}
```

### 4. Logging Best Practices

```csharp
// Use appropriate levels
await SendLogAsync(LoggingLevel.Debug, "Detailed debugging info");   // Development
await SendLogAsync(LoggingLevel.Info, "Normal operation");           // Production
await SendLogAsync(LoggingLevel.Warning, "Something unexpected");    // Important
await SendLogAsync(LoggingLevel.Error, "Operation failed");          // Critical
```

## Transport Modes

### STDIO (Default)

- **How it works**: Client spawns server as a child process
- **Communication**: stdin/stdout streams
- **Best for**: Local development, CLI tools, desktop apps
- **Features**: Full MCP support including sampling

```json
{
  "command": "dotnet",
  "args": ["run", "--project", "path/to/CSharpMcpDemo.csproj"]
}
```

### SSE (Server-Sent Events)

- **How it works**: HTTP-based with SSE for server→client messages
- **Communication**: HTTP POST for requests, SSE stream for notifications
- **Best for**: Web apps, cloud deployments, remote access
- **Trade-offs**: May not support sampling in stateless mode

## Project Structure Explanation

### Program.cs
Entry point that sets up the host with dependency injection:
```csharp
services.AddHostedService<FileAnalysisMcpServer>();
```

### FileAnalysisMcpServer.cs
Main implementation with:
- `OnInitialize`: Declares capabilities (Tools, Roots, Logging)
- `OnToolsList`: Returns available tools
- `OnToolCall`: Executes tool requests
- `OnRootsList`: Provides approved file system roots

## Dependencies

From `ModelContextProtocol` NuGet package:
- `ModelContextProtocol.Protocol` - Core protocol types
- `ModelContextProtocol.Server` - Server-side implementation
- `Microsoft.Extensions.Hosting` - Background service hosting

## Testing the Server

### Quick Test Commands

1. **Test Progress Notifications**:
   ```
   Create a large text file and ask Claude to analyze it
   ```

2. **Test Roots Security**:
   ```
   Try to read a file outside your Documents folder
   ```

3. **Test Logging**:
   ```
   Search for text in files and check the logs
   ```

4. **Test Combined Features**:
   ```
   Batch process multiple files and observe progress + logs
   ```

## Common Scenarios

### Scenario 1: Long-Running Operation
When your tool needs >1 second to complete, use progress notifications:
```csharp
var progressToken = request.Meta?.ProgressToken;
if (progressToken != null)
{
    await SendProgressAsync(progressToken, current, total);
}
```

### Scenario 2: File Access Control
Always validate file paths against approved roots:
```csharp
if (!IsPathWithinRoots(filePath, _approvedRoots))
{
    return CreateErrorResult("Access denied");
}
```

### Scenario 3: AI-Powered Analysis
Delegate complex analysis to the client's AI:
```csharp
var samplingRequest = new SamplingCreateMessageRequest { /* ... */ };
// Client's LLM processes the request
```

## Troubleshooting

### Server Won't Start
- Check .NET 10.0 SDK is installed: `dotnet --version`
- Verify dependencies: `dotnet restore`
- Check logs in stderr

### Tools Not Appearing
- Verify client is MCP-compatible
- Check initialization handshake succeeded
- Review client configuration file

### File Access Denied
- This is expected! Files must be within approved roots
- Check `OnRootsList` method for configured roots
- Request access to specific directories from client

### Progress Not Showing
- Client must include `progressToken` in request metadata
- Verify notifications are being sent: check logs
- Some clients may not display progress UI

## Learn More

- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [MCP GitHub](https://github.com/modelcontextprotocol)
- [C# ModelContextProtocol SDK](https://www.nuget.org/packages/ModelContextProtocol/)

## License

MIT License - See reference project for details.

## Contributing

This is a demonstration project. Feel free to:
- Add more tools showcasing other MCP features
- Improve error handling and logging
- Add unit tests
- Extend with additional C# API examples

## Author

Created as a demonstration for C# developers learning MCP advanced topics.

---

**Note**: This project is designed for educational purposes to help C# developers understand advanced MCP concepts through practical examples.
