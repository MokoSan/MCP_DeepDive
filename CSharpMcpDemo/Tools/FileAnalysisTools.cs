using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace CSharpMcpDemo.Tools;

/// <summary>
/// MCP Tools demonstrating advanced features:
/// - Progress Notifications: Real-time progress updates for long operations (see AnalyzeLargeFile)
/// - Roots: Secure file system access with permission boundaries (see ReadFileSecure)
/// - Logging: Structured log messages at different levels (see SearchFiles)
/// - Sampling: AI-assisted analysis (see AiSummarizeFiles - requires client support)
/// - Message Types: Proper use of Requests/Responses (all tools) and Notifications (progress, logs)
/// </summary>
[McpServerToolType]
public class FileAnalysisTools
{
    private readonly ILogger<FileAnalysisTools> _logger;
    private readonly IMcpServerContext _mcpContext;
    private static readonly List<string> ApprovedRoots = new()
    {
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Path.GetTempPath()
    };

    public FileAnalysisTools(ILogger<FileAnalysisTools> logger, IMcpServerContext mcpContext)
    {
        _logger = logger;
        _mcpContext = mcpContext;
    }

    /// <summary>
    /// DEMO: Progress Notifications
    /// Analyzes a large file with progress updates using MCP progress notifications
    /// Client must include progressToken in request metadata to receive updates
    /// </summary>
    [McpServerTool(Name = "analyze_large_file")]
    [Description("Analyzes a large file with progress notifications. Demonstrates progress tracking for long-running operations.")]
    public async Task<string> AnalyzeLargeFile(
        [Description("Absolute path to the file to analyze")] string filePath,
        [Description("Progress token for notifications (optional)")] string? progressToken = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analysis of: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            return $"Error: File not found: {filePath}";
        }

        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        var totalLines = lines.Length;
        var results = new StringBuilder();

        results.AppendLine($"File Analysis Results for: {Path.GetFileName(filePath)}");
        results.AppendLine($"Total Lines: {totalLines}");
        results.AppendLine();

        // Send initial progress notification: 0% complete
        if (progressToken != null)
        {
            _mcpContext.SendProgressNotification(progressToken, 0, totalLines);
            _logger.LogInformation("Sent progress: 0/{Total}", totalLines);
        }

        // Process lines and count statistics
        var wordCount = 0;
        var charCount = 0;
        var lastProgressPercent = 0;

        for (int i = 0; i < totalLines; i++)
        {
            var line = lines[i];
            wordCount += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            charCount += line.Length;

            // Send MCP progress notifications every 10%
            var currentPercent = (i + 1) * 100 / totalLines;
            if (currentPercent >= lastProgressPercent + 10 || i == totalLines - 1)
            {
                lastProgressPercent = currentPercent;
                
                // Send MCP Progress Notification
                if (progressToken != null)
                {
                    _mcpContext.SendProgressNotification(progressToken, i + 1, totalLines);
                }
                
                _logger.LogInformation("Progress: {Current}/{Total} ({Percent}%)",
                    i + 1, totalLines, currentPercent);
            }

            // Simulate processing time for demo
            if (i % 100 == 0)
            {
                await Task.Delay(10, cancellationToken);
            }
        }

        results.AppendLine($"Word Count: {wordCount}");
        results.AppendLine($"Character Count: {charCount}");
        results.AppendLine($"Average Line Length: {(double)charCount / totalLines:F2}");
        results.AppendLine($"Average Words Per Line: {(double)wordCount / totalLines:F2}");

        _logger.LogInformation("Analysis completed successfully");
        return results.ToString();
    }

    /// <summary>
    /// DEMO: Roots Security
    /// Reads a file only if it's within approved roots
    /// </summary>
    [McpServerTool(Name = "read_file_secure")]
    [Description("Reads a file within approved roots only. Demonstrates roots-based security.")]
    public async Task<string> ReadFileSecure(
        [Description("Path to file (must be within approved roots)")] string filePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to read file: {FilePath}", filePath);

        // Check if file is within approved roots
        if (!IsPathWithinRoots(filePath))
        {
            _logger.LogWarning("Access denied: {FilePath} is outside approved roots", filePath);
            return $"Error: Access denied. File is outside approved roots. Approved roots: {string.Join(", ", ApprovedRoots)}";
        }

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {FilePath}", filePath);
            return $"Error: File not found: {filePath}";
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        _logger.LogInformation("Successfully read {Length} characters from {FileName}",
            content.Length, Path.GetFileName(filePath));

        return $"File: {Path.GetFileName(filePath)}\n\n{content}";
    }

    /// <summary>
    /// DEMO: Sampling (AI-assisted analysis)
    /// This demonstrates MCP Sampling - server requests LLM completion from the client
    /// Server sends sampling/createMessage request and client executes using its configured LLM
    /// </summary>
    [McpServerTool(Name = "ai_summarize_files")]
    [Description("Uses client's AI (sampling) to summarize multiple files. Demonstrates server-requested AI inference.")]
    public async Task<string> AiSummarizeFiles(
        McpServer server,
        [Description("Directory containing files to summarize")] string directoryPath,
        [Description("File pattern (e.g., *.cs, *.txt)")] string filePattern = "*.txt",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AI summarization of files in: {DirectoryPath}", directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            return $"Error: Directory not found: {directoryPath}";
        }

        var files = Directory.GetFiles(directoryPath, filePattern);

        if (files.Length == 0)
        {
            _logger.LogWarning("No files matching pattern '{Pattern}' found in {Directory}",
                filePattern, directoryPath);
            return $"No files found matching pattern: {filePattern}";
        }

        _logger.LogInformation("Found {Count} files to analyze", files.Length);

        // Prepare content for AI analysis via MCP Sampling
        var filesContent = new StringBuilder();
        filesContent.AppendLine($"Analyze and summarize these {Math.Min(files.Length, 10)} files:");
        filesContent.AppendLine();

        foreach (var file in files.Take(10)) // Limit to 10 files for demo
        {
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            filesContent.AppendLine($"=== File: {Path.GetFileName(file)} ===");
            filesContent.AppendLine(content.Length > 1000
                ? content.Substring(0, 1000) + "..."
                : content);
            filesContent.AppendLine();
        }

        try
        {
            _logger.LogInformation("Attempting MCP Sampling request to client's LLM...");
            
            // Build MCP Sampling Request using proper SDK types
            var samplingParams = new CreateMessageRequestParams
            {
                Messages = [new SamplingMessage
                {
                    Role = Role.User,
                    Content = [new TextContentBlock { Text = filesContent.ToString() }],
                }],
                MaxTokens = 2000,
                Temperature = 0.7f,
                SystemPrompt = "You are a code analysis assistant. Provide a concise summary of the files provided.",
                ModelPreferences = new ModelPreferences
                {
                    Hints = [new ModelHint { Name = "claude-3-5-sonnet-20241022" }]
                }
            };

            // Send sampling request to client using the McpServer instance
            var response = await server.SampleAsync(samplingParams, cancellationToken);
            
            _logger.LogInformation("Received LLM response from client");
            
            var result = new StringBuilder();
            result.AppendLine("=== AI SUMMARIZATION RESULT ===");
            result.AppendLine();
            result.AppendLine($"Files analyzed: {Math.Min(files.Length, 10)} of {files.Length}");
            result.AppendLine($"Pattern: {filePattern}");
            result.AppendLine();
            result.AppendLine("AI Summary:");
            
            // Extract text from response content blocks
            var textContent = (response.Content.FirstOrDefault() as TextContentBlock)?.Text 
                ?? "No text content in response";
            result.AppendLine(textContent);
            
            result.AppendLine();
            result.AppendLine("Files:");
            foreach (var file in files.Take(10))
            {
                result.AppendLine($"  - {Path.GetFileName(file)} ({new FileInfo(file).Length:N0} bytes)");
            }
            
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Sampling request failed (client may not support sampling): {Message}", ex.Message);
            
            // Fallback response showing what would have been sent
            var result = new StringBuilder();
            result.AppendLine("=== MCP SAMPLING DEMONSTRATION ===");
            result.AppendLine();
            result.AppendLine("Note: Sampling request failed. Client may not support 'sampling' capability.");
            result.AppendLine();
            result.AppendLine("MCP Request Method: 'sampling/createMessage'");
            result.AppendLine($"Files to analyze: {files.Length} (showing first {Math.Min(files.Length, 10)})");
            result.AppendLine($"Pattern: {filePattern}");
            result.AppendLine();
            result.AppendLine("How MCP Sampling Works:");
            result.AppendLine("1. Server sends 'sampling/createMessage' request to client");
            result.AppendLine("2. Client executes prompt using its configured LLM (e.g., Claude, GPT-4)");
            result.AppendLine("3. Client returns LLM response to server");
            result.AppendLine("4. Server uses the AI-generated summary");
            result.AppendLine();
            result.AppendLine("Benefits:");
            result.AppendLine("- Server offloads AI inference cost to client");
            result.AppendLine("- Leverages client's already-configured LLM");
            result.AppendLine("- No server-side AI infrastructure needed");
            result.AppendLine();
            result.AppendLine("Files Found:");
            foreach (var file in files)
            {
                result.AppendLine($"  - {Path.GetFileName(file)} ({new FileInfo(file).Length:N0} bytes)");
            }
            result.AppendLine();
            result.AppendLine("To enable: Client must declare 'sampling' capability during initialization.");
            
            return result.ToString();
        }
    }

    /// <summary>
    /// DEMO: Structured Logging
    /// Searches files with detailed logging at different levels
    /// </summary>
    [McpServerTool(Name = "search_files")]
    [Description("Searches files for a pattern with detailed logging. Demonstrates structured logging.")]
    public async Task<string> SearchFiles(
        [Description("Root directory to search")] string rootPath,
        [Description("Text pattern to search for")] string searchPattern,
        [Description("File extension filter (e.g., .cs, .txt)")] string fileExtension = ".txt",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting file search in: {RootPath}", rootPath);
        _logger.LogDebug("Search params - Pattern: '{Pattern}', Extension: {Extension}",
            searchPattern, fileExtension);

        if (!Directory.Exists(rootPath))
        {
            _logger.LogError("Directory does not exist: {RootPath}", rootPath);
            return $"Error: Directory not found: {rootPath}";
        }

        var results = new List<string>();
        var files = Directory.GetFiles(rootPath, $"*{fileExtension}", SearchOption.AllDirectories);

        _logger.LogInformation("Scanning {Count} files...", files.Length);

        int filesScanned = 0;
        int matchesFound = 0;

        foreach (var file in files)
        {
            filesScanned++;

            try
            {
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        matchesFound++;
                        results.Add($"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");

                        if (matchesFound == 1)
                        {
                            _logger.LogInformation("First match found!");
                        }
                    }
                }

                if (filesScanned % 10 == 0)
                {
                    _logger.LogDebug("Progress: {Scanned}/{Total} files scanned, {Matches} matches",
                        filesScanned, files.Length, matchesFound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not read file {FileName}: {Message}",
                    Path.GetFileName(file), ex.Message);
            }
        }

        _logger.LogInformation("Search complete. Found {Matches} matches in {Scanned} files",
            matchesFound, filesScanned);

        var resultText = new StringBuilder();
        resultText.AppendLine($"Search Results for '{searchPattern}'");
        resultText.AppendLine($"Directory: {rootPath}");
        resultText.AppendLine($"Files scanned: {filesScanned}");
        resultText.AppendLine($"Matches found: {matchesFound}");
        resultText.AppendLine();

        if (matchesFound > 0)
        {
            resultText.AppendLine("Matches:");
            foreach (var match in results.Take(50)) // Limit output
            {
                resultText.AppendLine($"  {match}");
            }

            if (results.Count > 50)
            {
                resultText.AppendLine($"  ... and {results.Count - 50} more matches");
            }
        }
        else
        {
            resultText.AppendLine("No matches found.");
        }

        return resultText.ToString();
    }

    /// <summary>
    /// DEMO: Combined Features
    /// Batch processes files demonstrating progress, logging, and structured results
    /// </summary>
    [McpServerTool(Name = "batch_process_files")]
    [Description("Processes multiple files with progress and logging. Demonstrates all features together.")]
    public async Task<string> BatchProcessFiles(
        [Description("Directory containing files")] string directoryPath,
        [Description("Operation to perform: count_lines, find_todos, or analyze_complexity")] string operation,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting batch operation '{Operation}' on: {Directory}",
            operation, directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogError("Directory not found: {Directory}", directoryPath);
            return $"Error: Directory not found: {directoryPath}";
        }

        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            _logger.LogWarning("No .cs files found");
            return "No C# files found in directory.";
        }

        _logger.LogInformation("Processing {Count} files...", files.Length);

        var results = new Dictionary<string, int>();
        var processed = 0;

        foreach (var file in files)
        {
            processed++;

            try
            {
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var result = operation switch
                {
                    "count_lines" => content.Split('\n').Length,
                    "find_todos" => content.Split("TODO", StringSplitOptions.None).Length - 1,
                    "analyze_complexity" => content.Split('{').Length - 1, // Simple brace count
                    _ => 0
                };

                results[Path.GetFileName(file)] = result;

                // Log every 10th file
                if (processed % 10 == 0)
                {
                    _logger.LogDebug("Processed {Processed}/{Total} files", processed, files.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error processing {FileName}: {Message}",
                    Path.GetFileName(file), ex.Message);
            }

            await Task.Delay(20, cancellationToken); // Simulate work
        }

        _logger.LogInformation("Batch processing complete. Processed {Processed} files", processed);

        var output = new StringBuilder();
        output.AppendLine($"Batch Operation: {operation}");
        output.AppendLine($"Files Processed: {processed}");
        output.AppendLine();
        output.AppendLine("Results:");

        foreach (var kvp in results.OrderByDescending(x => x.Value).Take(20))
        {
            output.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }

        if (results.Count > 20)
        {
            output.AppendLine($"  ... and {results.Count - 20} more files");
        }

        return output.ToString();
    }

    /// <summary>
    /// Validates if a file path is within approved roots
    /// </summary>
    private bool IsPathWithinRoots(string filePath)
    {
        if (ApprovedRoots.Count == 0)
        {
            _logger.LogWarning("No roots configured - allowing access");
            return true;
        }

        try
        {
            var fullPath = Path.GetFullPath(filePath);
            return ApprovedRoots.Any(root =>
                fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }
}
