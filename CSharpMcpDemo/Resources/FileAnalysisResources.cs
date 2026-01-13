using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace CSharpMcpDemo.Resources;

/// <summary>
/// MCP Resources demonstrating resource capabilities.
/// Resources represent data or content that can be read by clients.
/// They provide structured access to information without executing tools.
/// </summary>
[McpServerResourceType]
public class FileAnalysisResources
{
    private readonly string _projectRoot;

    public FileAnalysisResources()
    {
        // Set project root to the demo project directory
        _projectRoot = Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// DEMO: Static Resource
    /// Returns project information as a resource
    /// </summary>
    [McpServerResource(Name = "project_info")]
    [Description("Provides information about the CSharp MCP Demo project, including version, features, and capabilities.")]
    public string GetProjectInfo()
    {
        var info = new
        {
            Name = "CSharp MCP Demo",
            Version = "1.0.0",
            Description = "Demonstration of advanced MCP features for C# developers",
            Features = new[]
            {
                "Sampling - AI-assisted analysis",
                "Roots - Secure file access",
                "Progress - Real-time updates",
                "Logging - Structured logs",
                "Prompts - Reusable prompt templates",
                "Resources - Structured data access"
            },
            Tools = new[]
            {
                "analyze_large_file",
                "read_file_secure",
                "ai_summarize_files",
                "search_files",
                "batch_process_files"
            },
            Prompts = new[]
            {
                "code_review_assistant",
                "refactoring_guide",
                "performance_optimizer",
                "documentation_writer",
                "test_strategist"
            },
            Documentation = "See README.md for comprehensive documentation"
        };

        return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// DEMO: Dynamic Resource
    /// Returns current system statistics
    /// </summary>
    [McpServerResource(Name = "system_stats")]
    [Description("Provides current system statistics including memory usage, CPU info, and process information.")]
    public string GetSystemStats()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        
        var stats = new
        {
            Timestamp = DateTime.UtcNow,
            Process = new
            {
                Id = process.Id,
                Name = process.ProcessName,
                StartTime = process.StartTime,
                Memory = new
                {
                    WorkingSetMB = process.WorkingSet64 / 1024.0 / 1024.0,
                    PrivateMemoryMB = process.PrivateMemorySize64 / 1024.0 / 1024.0,
                    VirtualMemoryMB = process.VirtualMemorySize64 / 1024.0 / 1024.0
                },
                Threads = process.Threads.Count,
                Handles = process.HandleCount
            },
            System = new
            {
                OS = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                MachineName = Environment.MachineName,
                DotNetVersion = Environment.Version.ToString(),
                Is64Bit = Environment.Is64BitOperatingSystem
            }
        };

        return JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// DEMO: File-based Resource
    /// Returns best practices guide
    /// </summary>
    [McpServerResource(Name = "best_practices_guide")]
    [Description("Comprehensive guide to C# and .NET best practices for modern development.")]
    public string GetBestPracticesGuide()
    {
        var guide = new StringBuilder();
        
        guide.AppendLine("# C# Best Practices Guide");
        guide.AppendLine();
        guide.AppendLine("## Coding Standards");
        guide.AppendLine();
        guide.AppendLine("### Naming Conventions");
        guide.AppendLine("- **Classes/Methods**: PascalCase (`CustomerService`, `GetCustomerById`)");
        guide.AppendLine("- **Parameters/Variables**: camelCase (`customerId`, `firstName`)");
        guide.AppendLine("- **Constants**: PascalCase (`MaxRetryCount`)");
        guide.AppendLine("- **Private fields**: _camelCase with underscore (`_logger`, `_repository`)");
        guide.AppendLine();
        guide.AppendLine("### Async/Await");
        guide.AppendLine("- Always use `async`/`await` for I/O-bound operations");
        guide.AppendLine("- Suffix async methods with `Async` (`GetDataAsync`)");
        guide.AppendLine("- Use `ConfigureAwait(false)` in library code");
        guide.AppendLine("- Prefer `Task` over `void` for async methods (except event handlers)");
        guide.AppendLine();
        guide.AppendLine("### Error Handling");
        guide.AppendLine("- Use specific exception types");
        guide.AppendLine("- Don't catch exceptions you can't handle");
        guide.AppendLine("- Use `using` statements for IDisposable resources");
        guide.AppendLine("- Log exceptions with full context");
        guide.AppendLine();
        guide.AppendLine("### Performance");
        guide.AppendLine("- Avoid premature optimization");
        guide.AppendLine("- Use `StringBuilder` for string concatenation in loops");
        guide.AppendLine("- Prefer `Span<T>` for performance-critical code");
        guide.AppendLine("- Cache expensive operations");
        guide.AppendLine("- Use `async` for I/O, not CPU-bound work");
        guide.AppendLine();
        guide.AppendLine("### Dependency Injection");
        guide.AppendLine("- Register services with appropriate lifetime (Singleton, Scoped, Transient)");
        guide.AppendLine("- Inject interfaces, not concrete types");
        guide.AppendLine("- Avoid service locator pattern");
        guide.AppendLine("- Keep constructors simple (don't do work)");
        guide.AppendLine();
        guide.AppendLine("### Testing");
        guide.AppendLine("- Write unit tests for business logic");
        guide.AppendLine("- Use AAA pattern (Arrange, Act, Assert)");
        guide.AppendLine("- Mock external dependencies");
        guide.AppendLine("- Aim for >80% code coverage on critical paths");

        return guide.ToString();
    }

    /// <summary>
    /// DEMO: Configuration Resource
    /// Returns MCP server configuration
    /// </summary>
    [McpServerResource(Name = "mcp_configuration")]
    [Description("MCP server configuration including approved roots, capabilities, and settings.")]
    public string GetMcpConfiguration()
    {
        var config = new
        {
            Server = new
            {
                Name = "csharp-mcp-demo",
                Version = "1.0.0",
                Protocol = "2024-11-05"
            },
            Capabilities = new
            {
                Tools = true,
                Prompts = true,
                Resources = true,
                Logging = true,
                Sampling = false // Client-dependent
            },
            ApprovedRoots = new[]
            {
                new
                {
                    Name = "My Documents",
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Purpose = "User document storage"
                },
                new
                {
                    Name = "Temp Directory",
                    Path = Path.GetTempPath(),
                    Purpose = "Temporary file operations"
                }
            },
            LoggingLevels = new[]
            {
                "Debug - Detailed diagnostic info",
                "Info - Normal operations",
                "Warning - Unexpected situations",
                "Error - Operation failures"
            }
        };

        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// DEMO: List-based Resource
    /// Returns available C# file patterns
    /// </summary>
    [McpServerResource(Name = "file_type_patterns")]
    [Description("Common C# file types and their purposes in a .NET project.")]
    public string GetFileTypePatterns()
    {
        var patterns = new
        {
            ProjectFiles = new[]
            {
                new { Pattern = "*.csproj", Description = "C# project file (MSBuild format)" },
                new { Pattern = "*.sln", Description = "Visual Studio solution file" },
                new { Pattern = "Directory.Build.props", Description = "Shared MSBuild properties" }
            },
            SourceFiles = new[]
            {
                new { Pattern = "*.cs", Description = "C# source code file" },
                new { Pattern = "*.cshtml", Description = "Razor view file (ASP.NET)" },
                new { Pattern = "*.razor", Description = "Blazor component file" }
            },
            ConfigurationFiles = new[]
            {
                new { Pattern = "appsettings.json", Description = "Application configuration" },
                new { Pattern = "appsettings.*.json", Description = "Environment-specific config" },
                new { Pattern = "launchSettings.json", Description = "Debug launch profiles" }
            },
            TestFiles = new[]
            {
                new { Pattern = "*Tests.cs", Description = "Unit test files" },
                new { Pattern = "*Test.cs", Description = "Test files (alternative naming)" },
                new { Pattern = "*.Tests.cs", Description = "Test files (namespace pattern)" }
            },
            DocumentationFiles = new[]
            {
                new { Pattern = "README.md", Description = "Project documentation" },
                new { Pattern = "CHANGELOG.md", Description = "Version history" },
                new { Pattern = "*.md", Description = "Markdown documentation" }
            }
        };

        return JsonSerializer.Serialize(patterns, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// DEMO: Template Resource
    /// Returns a code template
    /// </summary>
    [McpServerResource(Name = "async_method_template")]
    [Description("Template for creating a properly structured async method in C#.")]
    public string GetAsyncMethodTemplate()
    {
        return @"/// <summary>
/// [Brief description of what this method does]
/// </summary>
/// <param name=""parameter1"">[Description of parameter1]</param>
/// <param name=""cancellationToken"">Cancellation token for async operation</param>
/// <returns>[Description of return value]</returns>
/// <exception cref=""ArgumentNullException"">When parameter1 is null</exception>
/// <exception cref=""InvalidOperationException"">When [specific condition]</exception>
public async Task<TResult> MethodNameAsync(
    string parameter1,
    CancellationToken cancellationToken = default)
{
    // Validate parameters
    ArgumentNullException.ThrowIfNull(parameter1);
    
    // Log method entry (optional)
    _logger.LogInformation(""Starting {MethodName} with {Parameter}"", 
        nameof(MethodNameAsync), parameter1);
    
    try
    {
        // Perform async operation
        var result = await SomeAsyncOperation(parameter1, cancellationToken);
        
        // Process result
        var processedResult = ProcessResult(result);
        
        // Log success (optional)
        _logger.LogInformation(""{MethodName} completed successfully"", 
            nameof(MethodNameAsync));
        
        return processedResult;
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning(""{MethodName} was cancelled"", nameof(MethodNameAsync));
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, ""Error in {MethodName}"", nameof(MethodNameAsync));
        throw;
    }
}";
    }
}
