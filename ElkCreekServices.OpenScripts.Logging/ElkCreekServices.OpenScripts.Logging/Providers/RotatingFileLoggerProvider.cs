using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ElkCreekServices.OpenScripts.Logging.Providers;

/// <summary>
/// A logger provider that logs to a rotating file
/// </summary>
[UnsupportedOSPlatform("browser")]
[ProviderAlias("RotatingFile")]
public sealed class RotatingFileLoggerProvider : ILoggerProvider
{

    private IDisposable? _onchangeToken;
    private Configurations.Configuration _currentConfiguration;
    private readonly ConcurrentDictionary<string, RotatingFileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Constructor with options
    /// </summary>
    /// <param name="options"></param>
    public RotatingFileLoggerProvider(IOptionsMonitor<Configurations.Configuration> options)
    {
       _currentConfiguration = options.CurrentValue;
       _onchangeToken = options.OnChange((config, name) => _currentConfiguration = config);
    }   

    /// <summary>
    /// Create a logger
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new RotatingFileLogger(name, () => _currentConfiguration));

    /// <summary>
    /// Dispose
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Dispose()
    {
        //dispose all loggers in parallel to avoid extensive blocking
        var result = Parallel.ForEach(_loggers, logger => logger.Value.Dispose());
        if (!result.IsCompleted)
        {
            throw new Exception("Failed to dispose all loggers");
        }
        _loggers.Clear();
        _onchangeToken?.Dispose();
    }
}
