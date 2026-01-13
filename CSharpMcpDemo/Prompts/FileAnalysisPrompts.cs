using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CSharpMcpDemo.Prompts;

/// <summary>
/// MCP Prompts demonstrating prompt capabilities.
/// Prompts are reusable prompt templates that clients can use to guide LLM interactions.
/// They provide structured, expert guidance for specific tasks.
/// </summary>
[McpServerPromptType]
public class FileAnalysisPrompts
{
    /// <summary>
    /// DEMO: Code Review Prompt with Parameters
    /// A structured prompt for performing code reviews
    /// </summary>
    [McpServerPrompt(Name = "code_review_assistant")]
    [Description("Expert code review assistant that analyzes C# code for best practices, performance, security, and maintainability. Use this prompt when reviewing code changes.")]
    public string GetCodeReviewPrompt(
        [Description("The C# code to review")] string code,
        [Description("Focus area: correctness, performance, security, maintainability, or all")] string focusArea = "all",
        [Description("Project context or additional requirements (optional)")] string? context = null)
    {
        var prompt = @"You are an expert C# code reviewer with deep knowledge of:
- .NET best practices and design patterns
- Performance optimization techniques
- Security vulnerabilities (OWASP Top 10)
- Code maintainability and readability
- SOLID principles and clean code

Focus Area: " + focusArea.ToUpper() + @"

";

        if (!string.IsNullOrEmpty(context))
        {
            prompt += $"Project Context: {context}\n\n";
        }

        prompt += @"When reviewing code, analyze the following aspects:

1. **Correctness**: Does the code work as intended? Are there logical errors?
2. **Performance**: Are there inefficient algorithms or unnecessary allocations?
3. **Security**: Are there potential vulnerabilities (SQL injection, XSS, etc.)?
4. **Maintainability**: Is the code easy to understand and modify?
5. **Best Practices**: Does it follow C# conventions and .NET guidelines?

For each issue found, provide:
- **Severity**: Critical, High, Medium, Low
- **Location**: File and line number
- **Description**: Clear explanation of the issue
- **Recommendation**: Specific suggestion to fix it
- **Example**: Show corrected code when helpful

Be constructive and educational in your feedback.

Code to review:
```csharp
" + code + @"
```";

        return prompt;
    }

    /// <summary>
    /// DEMO: Refactoring Guidance Prompt with Parameters
    /// Helps guide refactoring decisions
    /// </summary>
    [McpServerPrompt(Name = "refactoring_guide")]
    [Description("Expert refactoring guide for improving code quality. Use when considering how to restructure or improve existing code.")]
    public string GetRefactoringPrompt(
        [Description("The C# code to refactor")] string code,
        [Description("Primary goal: readability, performance, maintainability, testability, or general")] string goal = "general",
        [Description("Known issues or constraints (optional)")] string? knownIssues = null)
    {
        var prompt = $@"You are an expert software architect specializing in code refactoring and improvement.

Refactoring Goal: {goal.ToUpper()}
";

        if (!string.IsNullOrEmpty(knownIssues))
        {
            prompt += $"\nKnown Issues/Constraints: {knownIssues}\n";
        }

        prompt += @"
When analyzing code for refactoring opportunities, consider:

**Code Smells to Identify:**
- Long methods (>20 lines)
- Large classes (>300 lines)
- Duplicated code
- Long parameter lists (>3 parameters)
- Feature envy (accessing other objects' data frequently)
- Primitive obsession (using primitives instead of small objects)

**Refactoring Techniques:**
- Extract Method: Break down long methods
- Extract Class: Split responsibilities
- Introduce Parameter Object: Group related parameters
- Replace Conditional with Polymorphism
- Move Method/Field: Better object placement
- Rename: Improve clarity

**Priorities:**
1. Start with high-impact, low-risk refactorings
2. Ensure tests exist before refactoring
3. Make small, incremental changes
4. Validate after each refactoring step

Provide specific, actionable recommendations with code examples.

Code to refactor:
```csharp
" + code + @"
```";

        return prompt;
    }

    /// <summary>
    /// DEMO: Performance Analysis Prompt with Parameters
    /// Guides performance optimization analysis
    /// </summary>
    [McpServerPrompt(Name = "performance_optimizer")]
    [Description("Performance optimization expert for .NET applications. Use when analyzing code for performance improvements.")]
    public string GetPerformancePrompt(
        [Description("The C# code to optimize")] string code,
        [Description("Performance concern: allocations, collections, async, io, database, threading, or general")] string concern = "general",
        [Description("Target: hot path, startup, throughput, or latency (optional)")] string? target = null,
        [Description("Current performance metrics if available (optional)")] string? metrics = null)
    {
        var prompt = $@"You are a .NET performance optimization expert.

Performance Concern: {concern.ToUpper()}
";

        if (!string.IsNullOrEmpty(target))
        {
            prompt += $"Optimization Target: {target}\n";
        }

        if (!string.IsNullOrEmpty(metrics))
        {
            prompt += $"Current Metrics: {metrics}\n";
        }

        prompt += @"
When analyzing code for performance, focus on:

**Common Performance Issues:**
1. **Allocations**: Unnecessary object creation, boxing, string concatenations
2. **Collections**: Wrong collection types, inefficient LINQ usage
3. **Async/Await**: Sync-over-async, excessive Task creation
4. **I/O**: Synchronous I/O, missing buffering, excessive file operations
5. **Database**: N+1 queries, missing indexes, inefficient queries
6. **Threading**: Lock contention, thread pool starvation

**Optimization Strategies:**
- Use `StringBuilder` for multiple string operations
- Prefer `List<T>` over `LinkedList<T>` unless specific needs
- Use `Span<T>` and `Memory<T>` for performance-critical code
- Avoid LINQ in hot paths (use loops instead)
- Use `async`/`await` properly (ConfigureAwait, ValueTask)
- Pool objects with `ArrayPool<T>` or `ObjectPool<T>`
- Cache expensive computations

**Measurement First:**
- Always measure before optimizing (BenchmarkDotNet)
- Focus on actual bottlenecks, not premature optimization
- Consider readability vs. performance trade-offs

Provide specific recommendations with performance impact estimates.

Code to analyze:
```csharp
" + code + @"
```";

        return prompt;
    }

    /// <summary>
    /// DEMO: Documentation Writer Prompt with Parameters
    /// Helps generate comprehensive documentation
    /// </summary>
    [McpServerPrompt(Name = "documentation_writer")]
    [Description("Technical documentation expert for C# code. Use when generating or improving code documentation.")]
    public string GetDocumentationPrompt(
        [Description("The C# code to document")] string code,
        [Description("Documentation type: xml-comments, readme, api-docs, or tutorial")] string docType = "xml-comments",
        [Description("Target audience: beginner, intermediate, or expert")] string audience = "intermediate",
        [Description("Additional context about the code's purpose (optional)")] string? purpose = null)
    {
        var prompt = $@"You are a technical writer specializing in C# and .NET documentation.

Documentation Type: {docType.ToUpper()}
Target Audience: {audience.ToUpper()}
";

        if (!string.IsNullOrEmpty(purpose))
        {
            prompt += $"\nCode Purpose: {purpose}\n";
        }

        prompt += @"
When documenting code, include:

**XML Documentation Comments:**
```csharp
/// <summary>
/// Brief one-line description of the method/class.
/// </summary>
/// <param name=""paramName"">Description of parameter and its purpose</param>
/// <returns>Description of return value and possible values</returns>
/// <exception cref=""ExceptionType"">When this exception is thrown</exception>
/// <remarks>
/// Additional details, usage notes, or examples.
/// </remarks>
/// <example>
/// <code>
/// var result = MyMethod(""example"");
/// </code>
/// </example>
```

**README Sections:**
1. **Overview**: What the project does
2. **Installation**: How to get started
3. **Usage**: Common use cases with examples
4. **API Reference**: Key classes and methods
5. **Configuration**: Settings and options
6. **Troubleshooting**: Common issues and solutions
7. **Contributing**: How to contribute
8. **License**: Licensing information

**Best Practices:**
- Write for the audience (beginners vs. experts)
- Include code examples for complex features
- Keep documentation up to date with code changes
- Explain the 'why', not just the 'what'
- Use clear, concise language

Generate documentation that is helpful, accurate, and easy to understand.

Code to document:
```csharp
" + code + @"
```";

        return prompt;
    }

    /// <summary>
    /// DEMO: Test Strategy Prompt with Parameters
    /// Guides test planning and implementation
    /// </summary>
    [McpServerPrompt(Name = "test_strategist")]
    [Description("Testing strategy expert for C# applications. Use when planning or improving test coverage.")]
    public string GetTestStrategyPrompt(
        [Description("The C# code to test")] string code,
        [Description("Test level: unit, integration, e2e, or comprehensive")] string testLevel = "unit",
        [Description("Testing framework: xunit, nunit, mstest, or any")] string framework = "xunit",
        [Description("Specific scenarios to cover (optional)")] string? scenarios = null)
    {
        var prompt = $@"You are a testing expert specializing in C# and .NET testing strategies.

Test Level: {testLevel.ToUpper()}
Preferred Framework: {framework}
";

        if (!string.IsNullOrEmpty(scenarios))
        {
            prompt += $"\nSpecific Scenarios to Cover: {scenarios}\n";
        }

        prompt += @"
When analyzing testing needs, consider:

**Testing Pyramid:**
1. **Unit Tests (70%)**: Fast, isolated, test individual methods
   - Use xUnit, NUnit, or MSTest
   - Mock dependencies with Moq or NSubstitute
   - Aim for high code coverage on business logic

2. **Integration Tests (20%)**: Test component interactions
   - Database integration with TestContainers
   - API integration tests with WebApplicationFactory
   - Message queue integration

3. **End-to-End Tests (10%)**: Full application workflows
   - UI testing with Selenium or Playwright
   - API workflow testing
   - Performance testing

**Test Patterns:**
- **AAA Pattern**: Arrange, Act, Assert
- **Given-When-Then**: BDD style tests
- **Test Data Builders**: For complex test data
- **Test Fixtures**: Shared setup/teardown

**What to Test:**
- Happy path (normal flow)
- Edge cases (boundary conditions)
- Error handling (exceptions, validation)
- Concurrency scenarios (race conditions)
- Performance characteristics

**Best Practices:**
- Test behavior, not implementation
- Keep tests simple and focused
- Use descriptive test names
- Avoid test interdependencies
- Fast feedback (tests should run quickly)

Provide specific test scenarios and implementation guidance.

Code to test:
```csharp
" + code + @"
```";

        return prompt;
    }
}
