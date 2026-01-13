# MCP Advanced Topics - Precise Notes

## 1. Sampling

**Definition**: Sampling allows MCP servers to request LLM completions from the client, enabling the server to offload AI inference costs and complexity to the client application.

**Key Points**:
- **Direction**: Server → Client request for LLM completion
- **Purpose**: Servers can delegate AI reasoning to the connected client's LLM
- **Use Cases**:
  - Server needs AI assistance for complex decision-making
  - Cost distribution: client pays for LLM inference instead of server
  - Leveraging the client's already-configured LLM
- **Implementation**:
  - Server calls `sampling/createMessage` request
  - Client executes the LLM call and returns the result
  - Supports system prompts, messages, model preferences, and sampling parameters
- **Capability**: Client must declare `sampling` capability during initialization
- **Benefits**: Enables sophisticated agentic behavior without server-side AI infrastructure

**C# Example (Server-side)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

// Server requesting sampling from client
public class MyMcpServer : McpServer
{
    public async Task<string> ProcessWithAI(string userQuery)
    {
        // Server requests LLM completion from client
        var samplingRequest = new SamplingCreateMessageRequest
        {
            Messages = new[]
            {
                new SamplingMessage
                {
                    Role = "user",
                    Content = new TextContent { Text = userQuery }
                }
            },
            ModelPreferences = new ModelPreferences
            {
                Hints = new[] { new ModelHint { Name = "claude-3-5-sonnet-20241022" } }
            },
            MaxTokens = 1000
        };

        var response = await SendSamplingRequestAsync(samplingRequest);
        return response.Content.Text;
    }
}
```

---

## 2. Roots

**Definition**: Roots define the file system directories or URIs that an MCP server is permitted to access, implementing a security boundary for file operations.

**Key Points**:
- **Purpose**: Explicit permission model for file system access
- **Security**: Prevents unauthorized file access outside designated boundaries
- **Declaration**: Server declares supported roots during initialization
- **Client Control**: Client approves/configures which roots to grant
- **Structure**:
  - `uri`: The root directory URI (e.g., `file:///home/user/projects`)
  - `name`: Human-readable identifier
- **Common Pattern**: Used with file-related tools to scope operations
- **Best Practice**: Always validate file paths are within granted roots before operations
- **Use Case**: A code analysis server might request root access to a project directory

**C# Example (Server-side)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

public class FileServerWithRoots : McpServer
{
    protected override Task<InitializeResult> HandleInitializeAsync(InitializeRequest request)
    {
        return Task.FromResult(new InitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ServerCapabilities
            {
                Resources = new ResourcesCapability(),
                Roots = new RootsCapability
                {
                    ListChanged = true // Notify when roots change
                }
            },
            ServerInfo = new Implementation
            {
                Name = "file-server",
                Version = "1.0.0"
            }
        });
    }

    // Validate file access within approved roots
    private bool IsPathWithinRoots(string filePath, List<Root> approvedRoots)
    {
        var fileUri = new Uri(filePath);
        return approvedRoots.Any(root => 
            fileUri.AbsolutePath.StartsWith(new Uri(root.Uri).AbsolutePath));
    }

    public async Task<string> ReadFileAsync(string path, List<Root> roots)
    {
        if (!IsPathWithinRoots(path, roots))
            throw new UnauthorizedAccessException("Path is outside approved roots");
        
        return await File.ReadAllTextAsync(path);
    }
}
```

---

## 3. Progress Logging and Notifications

### Progress Notifications
**Definition**: Mechanism for servers to report long-running operation progress to clients.

**Key Points**:
- **Type**: `notifications/progress`
- **Use Case**: Operations that take >1 second (file processing, API calls, large computations)
- **Unidirectional**: Server → Client (no response expected)
- **Parameters**:
  - `progressToken`: Identifier linking progress updates to the originating request
  - `progress`: Current progress value
  - `total`: Maximum progress value (optional)
- **UX Benefit**: Enables progress bars, status indicators in client UI
- **Pattern**: 
  1. Client includes `_meta.progressToken` in request
  2. Server sends progress notifications during execution
  3. Server completes with final response

### Logging
**Definition**: Server-to-client logging for debugging and monitoring.

**Key Points**:
- **Type**: `notifications/message`
- **Levels**: `debug`, `info`, `warning`, `error`
- **Purpose**: Diagnostic information, errors, operational status
- **Visibility**: Typically shown in client's debug/console view
- **Data**: Can include structured data attached to log messages
- **Best Practice**: Use appropriate levels; avoid spamming with excessive logs

**C# Example (Server-side)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

public class ProgressEnabledServer : McpServer
{
    // Send progress notifications
    public async Task<ToolResult> ProcessLargeFileAsync(ToolCallRequest request)
    {
        var progressToken = request.Meta?.ProgressToken;
        var totalItems = 100;

        for (int i = 0; i < totalItems; i++)
        {
            // Do work...
            await Task.Delay(50);

            // Send progress notification if token provided
            if (progressToken != null)
            {
                await SendProgressNotificationAsync(new ProgressNotification
                {
                    ProgressToken = progressToken,
                    Progress = i + 1,
                    Total = totalItems
                });
            }

            // Send log messages
            if (i % 25 == 0)
            {
                await SendLogMessageAsync(new LoggingMessageNotification
                {
                    Level = LoggingLevel.Info,
                    Data = $"Processed {i + 1}/{totalItems} items"
                });
            }
        }

        return new ToolResult
        {
            Content = new[] { new TextContent { Text = "Processing complete" } }
        };
    }
}
```

---

## 4. STDIO vs. SSE (Server-Sent Events, formerly StreamingHttp)

### STDIO Transport

**Characteristics**:
- **Mechanism**: Standard input/output streams (stdin/stdout)
- **Process Model**: Client spawns server as child process
- **Communication**: JSON-RPC over stdio streams
- **Statefulness**: Fully stateful - maintains persistent connection
- **Use Case**: Local development, desktop applications, CLI tools

**Nuances**:
- ✅ **Supports all MCP features**: Sampling, progress, notifications, bidirectional communication
- ✅ **Simple deployment**: Single executable
- ✅ **Process isolation**: Each client gets dedicated server instance
- ❌ **Not web-scalable**: Can't serve multiple remote clients
- ❌ **Platform-dependent**: Requires local process execution
- **Debugging**: stderr available for debug output separate from protocol messages

**C# Example (Client launching STDIO server)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Client;

public class StdioClientExample
{
    public async Task ConnectToStdioServerAsync()
    {
        // Client spawns server as child process
        var client = new McpClient(new StdioServerParameters
        {
            Command = "node",
            Arguments = new[] { "path/to/server.js" },
            Environment = new Dictionary<string, string>
            {
                { "LOG_LEVEL", "debug" }
            }
        });

        await client.ConnectAsync();
        
        // Initialize handshake
        var initResult = await client.InitializeAsync(new InitializeRequest
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ClientCapabilities
            {
                Sampling = new SamplingCapability(),
                Roots = new RootsCapability { ListChanged = true }
            },
            ClientInfo = new Implementation { Name = "my-client", Version = "1.0" }
        });

        Console.WriteLine($"Connected to: {initResult.ServerInfo.Name}");
    }
}
```

### SSE (Server-Sent Events) Transport

**Characteristics**:
- **Mechanism**: HTTP-based with SSE for server→client messages
- **Process Model**: Long-lived server process serving multiple clients
- **Communication**: 
  - Client→Server: HTTP POST requests
  - Server→Client: Server-Sent Events stream
- **Statefulness**: Can be stateless or stateful
- **Use Case**: Web applications, cloud deployments, remote access

**Nuances**:
- ✅ **Web-compatible**: Works in browsers and web environments
- ✅ **Scalable**: Can serve many clients from one server
- ✅ **Firewall-friendly**: Standard HTTP/HTTPS
- ⚠️ **Feature trade-offs** (if implementing stateless):
  - ❌ Sampling not supported (requires client context)
  - ❌ Progress notifications limited
  - ❌ Logging may be compromised
- ⚠️ **Complexity**: Requires session management, authentication, CORS handling
- ⚠️ **Stateless challenges**: Each request is independent, no persistent context
- **Best Practice**: Use stateful SSE when full features needed; stateless for simple tool-only servers

**Key Decision Point**: 
- **STDIO**: Full features, local use, development
- **SSE**: Production web deployments, accepting potential feature limitations for scalability

**C# Example (Client connecting to SSE server)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Client;

public class SseClientExample
{
    public async Task ConnectToSseServerAsync()
    {
        // Client connects to HTTP-based SSE server
        var client = new McpClient(new SseServerParameters
        {
            Url = "https://api.example.com/mcp",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer YOUR_TOKEN" }
            }
        });

        await client.ConnectAsync();
        
        var initResult = await client.InitializeAsync(new InitializeRequest
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ClientCapabilities
            {
                // Note: Sampling may not be supported in stateless SSE
                Roots = new RootsCapability()
            },
            ClientInfo = new Implementation { Name = "web-client", Version = "1.0" }
        });

        Console.WriteLine($"Connected via SSE to: {initResult.ServerInfo.Name}");
    }
}
```

---

## 5. Types of Messages from MCP Protocol

MCP uses **JSON-RPC 2.0** with three message categories:

### Request/Response Pattern

**Requests**:
- **Structure**: `{ jsonrpc: "2.0", id: number|string, method: string, params?: object }`
- **Direction**: Bidirectional (client↔server)
- **Characteristic**: Always expects a response
- **Examples**:
  - `initialize` - Handshake and capability negotiation
  - `tools/list` - Client requests available tools
  - `tools/call` - Client invokes a tool
  - `resources/read` - Client reads a resource
  - `sampling/createMessage` - Server requests LLM completion
- **ID**: Must be unique; used to match response to request

**Responses**:
- **Success**: `{ jsonrpc: "2.0", id: number|string, result: any }`
- **Error**: `{ jsonrpc: "2.0", id: number|string, error: { code: number, message: string, data?: any } }`
- **Matching**: Response `id` must match the request `id`
- **Timing**: Must eventually receive a response (or timeout)

### Notifications

**Structure**: `{ jsonrpc: "2.0", method: string, params?: object }`

**Characteristics**:
- **No ID field**: Distinguishes from requests
- **No Response**: Fire-and-forget, no acknowledgment expected
- **Direction**: Bidirectional (client↔server)
- **Purpose**: Asynchronous events, status updates, logging

**Common Notification Types**:

**Server → Client**:
- `notifications/message` - Log messages (debug, info, warning, error)
- `notifications/progress` - Progress updates for long operations
- `notifications/resources/updated` - Resource changed notification
- `notifications/tools/list_changed` - Tool list has changed

**Client → Server**:
- `notifications/cancelled` - Client cancelled a request
- `notifications/roots/list_changed` - Available roots changed

**Key Difference**: 
- **Request** = "Do this and tell me the result" (requires response)
- **Notification** = "FYI, this happened" (no response needed)

**C# Example (Handling both message types)**:
```csharp
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

public class MessageTypesExample : McpServer
{
    // REQUEST: Client calls tool, expects response
    protected override async Task<ToolCallResult> HandleToolCallAsync(ToolCallRequest request)
    {
        // This is a REQUEST - must return a response
        var result = await ExecuteToolAsync(request.Params.Name, request.Params.Arguments);
        
        return new ToolCallResult
        {
            Content = new[] { new TextContent { Text = result } }
        };
    }

    // NOTIFICATION: Send log without expecting response
    public async Task LogInfoAsync(string message)
    {
        // This is a NOTIFICATION - fire and forget, no response expected
        await SendNotificationAsync("notifications/message", new LoggingMessageNotification
        {
            Level = LoggingLevel.Info,
            Data = message
        });
    }

    // NOTIFICATION: Notify resource updated
    public async Task NotifyResourceChangedAsync(string resourceUri)
    {
        await SendNotificationAsync("notifications/resources/updated", new ResourceUpdatedNotification
        {
            Uri = resourceUri
        });
    }

    // CLIENT SIDE: Receiving notifications
    protected override Task HandleNotificationAsync(string method, object parameters)
    {
        switch (method)
        {
            case "notifications/cancelled":
                var cancelNotif = (CancelledNotification)parameters;
                Console.WriteLine($"Request {cancelNotif.RequestId} was cancelled");
                break;
            
            case "notifications/progress":
                var progressNotif = (ProgressNotification)parameters;
                Console.WriteLine($"Progress: {progressNotif.Progress}/{progressNotif.Total}");
                break;
        }
        return Task.CompletedTask;
    }
}
```

---

## Summary Matrix

| Feature | STDIO | SSE (Stateful) | SSE (Stateless) |
|---------|-------|----------------|-----------------|
| Requests/Responses | ✅ Full | ✅ Full | ✅ Full |
| Notifications | ✅ Full | ✅ Full | ⚠️ Limited |
| Sampling | ✅ Yes | ✅ Yes | ❌ No |
| Progress | ✅ Yes | ✅ Yes | ⚠️ Limited |
| Logging | ✅ Yes | ✅ Yes | ⚠️ Limited |
| Roots | ✅ Yes | ✅ Yes | ✅ Yes |
| Deployment | Local | Remote | Remote |
| Scalability | Low | Medium | High |

---

## Protocol Flow Example

```
1. Client → Server: Request (initialize)
2. Server → Client: Response (capabilities, including roots)
3. Client → Server: Notification (roots/list_changed)
4. Client → Server: Request (tools/list)
5. Server → Client: Response (available tools)
6. Client → Server: Request (tools/call with progressToken)
7. Server → Client: Notification (progress 0/100)
8. Server → Client: Notification (progress 50/100)
9. Server → Client: Notification (message - logging)
10. Server → Client: Response (final result)
```

---

## Teams Summary Table

| Topic | Summary | Key Benefit |
|-------|---------|-------------|
| **Sampling** | • Server requests LLM completions from client<br>• Offloads AI inference costs | • Servers leverage client's AI without running their own models |
| **Roots** | • Security permission model<br>• Defines which file system directories server can access | • Client controls and approves access boundaries<br>• Prevents unauthorized file access |
| **Progress Logging & Notifications** | • Servers send real-time progress updates<br>• Logging messages for debugging | • Improves UX with progress bars<br>• Provides diagnostic info |
| **STDIO vs. SSE** | • **STDIO**: Local, full-featured, child process<br>• **SSE**: Web-scalable, remote, stateless mode sacrifices features | • STDIO for dev/local use<br>• SSE for production web with trade-offs |
| **Message Types** | • **Request/Response**: Bidirectional Q&A with IDs, expects reply<br>• **Notifications**: Fire-and-forget, no response needed | • Clear protocol patterns<br>• Synchronous vs. asynchronous communication |


# Synopsis

1. Sampling
• Server requests LLM completions from client
• Offloads AI inference costs to client
• Enables servers to leverage client's AI without running their own models
• Requires client to declare sampling capability <- VS CODE HAS THIS! 
 
2. Roots
• Security permission model for file system access
• Defines which directories an MCP server can access
• Client controls and approves access boundaries
• Prevents unauthorized file access outside designated areas
 
3. Progress Logging & Notifications
• Servers send real-time progress updates (notifications/progress)
• Logging messages for debugging (notifications/message)
• Improves UX with progress bars and status indicators
• Provides diagnostic info (debug, info, warning, error levels)
 
4. STDIO vs. SSE - Now Streamable HTTP
• STDIO: Local, full-featured, spawns as child process, supports all MCP features
• Streamable HTTP / SSE: Web-scalable, remote access, HTTP-based
• Stateless SSE sacrifices sampling, progress, and logging for horizontal scalability
• STDIO for dev/local use; SSE for production web deployments
 
5. Message Types
• Request/Response: Bidirectional Q&A with unique IDs, expects reply (e.g., tools/call)
• Notifications: Fire-and-forget, no response needed, no ID field (e.g., progress updates, logs)
• Clear protocol patterns for synchronous vs. asynchronous communication
 