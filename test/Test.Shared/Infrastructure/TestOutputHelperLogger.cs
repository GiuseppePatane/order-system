using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Test.Shared.Infrastructure;

/// <summary>
/// Logger provider that writes to xUnit test output
/// </summary>
public class TestOutputHelperLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestOutputHelperLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestOutputHelperLogger(_testOutputHelper, categoryName);
    }

    public void Dispose() { }
}

/// <summary>
/// Logger implementation that writes to xUnit test output
/// </summary>
public class TestOutputHelperLogger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;

    public TestOutputHelperLogger(ITestOutputHelper testOutputHelper, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        try
        {
            var message = $"[{logLevel}] {_categoryName}: {formatter(state, exception)}";
            if (exception != null)
            {
                message += $"\n{exception}";
            }
            _testOutputHelper.WriteLine(message);
        }
        catch
        {
            // Ignore errors writing to test output
        }
    }
}
