# MCP Advanced Topics - C# Demonstration Project

A comprehensive demonstration of **Model Context Protocol (MCP)** advanced features implemented in C# for .NET developers. This project showcases sampling, roots-based security, progress notifications, structured logging, and transport mechanisms through practical file system operations.

## What is MCP?

The Model Context Protocol is an open standard that enables AI applications to securely connect with external data sources and tools. It provides a standardized way for AI assistants to interact with local services, files, and APIs while maintaining security boundaries.

## Project Overview

This demonstration project implements an MCP server that provides:

- **5 MCP Tools** - File analysis operations with advanced features
- **5 MCP Prompts** - Reusable AI-guided analysis templates
- **6 MCP Resources** - Structured data access for clients

## Key Features Demonstrated

### 1. Sampling

Allows the MCP server to request LLM completions from the connected client, shifting AI computational costs from server to client infrastructure.

```csharp
// Server requests AI summarization from client
var result = await mcpContext.SendSamplingRequestAsync(
    $"Summarize this file content:\n{fileContent}",
    cancellationToken);
```

### 2. Roots (Security Boundaries)

Implements file access control through approved directory roots, ensuring servers only access authorized locations.

```csharp
// Validates file access within approved roots
private bool IsPathWithinRoots(string filePath)
{
    var approvedRoots = new[] {
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Path.GetTempPath()
    };
    return approvedRoots.Any(root =>
        normalizedPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));
}
```

### 3. Progress Notifications

Real-time progress updates for long-running operations, enabling client-side progress bars and status indicators.

```csharp
// Send progress updates during file analysis
await mcpContext.SendProgressNotification(
    progressToken,
    progress: percentComplete,
    total: 100);
```

### 4. Structured Logging

Multi-level logging (Debug, Info, Warning, Error) that sends diagnostic information to connected clients.

```csharp
await mcpContext.SendLogNotification(
    LoggingLevel.Info,
    "FileSearch",
    $"Found {matches.Count} files matching pattern");
```

### 5. Transport Mechanisms

Configured for **STDIO transport** (JSON-RPC over stdin/stdout), ideal for local development and desktop applications.

## Project Structure

```
quizzical-kapitsa/
├── README.md                               # This file
├── MCP_Presentation.sln                    # Visual Studio solution
├── MCP_Advanced_Topics_Notes.md            # Detailed MCP concepts documentation
└── CSharpMcpDemo/
    ├── Program.cs                          # Application entry point
    ├── McpServerContext.cs                 # MCP protocol interface
    ├── CSharpMcpDemo.csproj                # Project configuration
    ├── appsettings.json                    # Server configuration
    ├── README.md                           # Feature-specific documentation
    ├── Tools/
    │   └── FileAnalysisTools.cs            # MCP Tools implementation
    ├── Prompts/
    │   └── FileAnalysisPrompts.cs          # MCP Prompts templates
    └── Resources/
        └── FileAnalysisResources.cs        # MCP Resources data access
```

## Available MCP Tools

| Tool | Description | Features Demonstrated |
|------|-------------|----------------------|
| `analyze_large_file` | Analyzes files with real-time progress | Progress Notifications, Logging |
| `read_file_secure` | Secure file reading within approved roots | Roots Security |
| `ai_summarize_files` | AI-powered file summarization | Sampling |
| `search_files` | Pattern-based file search | Structured Logging |
| `batch_process_files` | Batch C# file processing | Combined Features |

## Available MCP Prompts

| Prompt | Purpose |
|--------|---------|
| `code_review_assistant` | Structured C# code review guidance |
| `refactoring_guide` | Code smell identification and refactoring strategies |
| `performance_optimizer` | Performance analysis for .NET applications |
| `documentation_writer` | XML documentation and technical docs generation |
| `test_strategist` | Test coverage planning and implementation |

## Available MCP Resources

| Resource | Content |
|----------|---------|
| `project_info` | Project metadata and capabilities (JSON) |
| `system_stats` | Dynamic system information |
| `best_practices_guide` | C# and .NET best practices (Markdown) |
| `mcp_configuration` | Server configuration details |
| `file_type_patterns` | Common .NET file types reference |
| `async_method_template` | Async method code template |

## Prerequisites

- **.NET 10.0 SDK** or later
- **Visual Studio 2022** or VS Code with C# extension
- An MCP-compatible client (e.g., Claude Desktop)

## Getting Started

### 1. Clone and Build

```bash
git clone <repository-url>
cd quizzical-kapitsa
dotnet build
```

### 2. Run the Server

```bash
cd CSharpMcpDemo
dotnet run
```

### 3. Configure MCP Client

Add to your Claude Desktop configuration (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "csharp-demo": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/CSharpMcpDemo.csproj"]
    }
  }
}
```

### 4. Install as Global Tool (Optional)

```bash
dotnet pack
dotnet tool install --global --add-source ./nupkg CSharpMcpDemo
```

## Technology Stack

- **Runtime**: .NET 10.0
- **Framework**: ASP.NET Core with Microsoft.Extensions.Hosting
- **MCP SDK**: ModelContextProtocol 0.5.0-preview.1
- **Transport**: STDIO (JSON-RPC 2.0)

## MCP Message Types

### Request/Response Pattern
Used for operations that return values:
- `tools/call` - Execute a tool and return results
- `prompts/get` - Retrieve a prompt template
- `resources/read` - Read resource content

### Notification Pattern
Fire-and-forget messages for updates:
- `notifications/progress` - Progress updates
- `notifications/message` - Log messages

## Transport Comparison

| Feature | STDIO | SSE | HTTP |
|---------|-------|-----|------|
| Best For | Local/Desktop | Web Apps | APIs |
| Bidirectional | Yes | Server→Client | Request/Response |
| Sampling Support | Full | Full | Configurable |
| Scaling | Single Process | Stateful | Horizontal |

## Learning Resources

- **MCP_Advanced_Topics_Notes.md** - Comprehensive documentation of all MCP concepts
- **CSharpMcpDemo/README.md** - Detailed feature guide with code examples
- [MCP Specification](https://modelcontextprotocol.io/) - Official protocol documentation
- [Anthropic MCP Course](https://anthropic.skilljar.com/model-context-protocol-advanced-topics) - Advanced topics course

## Use Cases

This demonstration is ideal for:

1. **Learning MCP** - Understand advanced protocol features through working examples
2. **Reference Implementation** - Template for building production MCP servers
3. **Feature Testing** - Test MCP client implementations against a compliant server
4. **Educational Purposes** - Training material for teams adopting MCP

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns and style
- New features include appropriate documentation
- Tests cover new functionality

## License

This project is provided for educational and demonstration purposes.

---

**Built with the Model Context Protocol for C# developers**
