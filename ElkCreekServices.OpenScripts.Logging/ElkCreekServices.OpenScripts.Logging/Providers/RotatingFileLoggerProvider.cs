using System.Collections.Concurrent;
using System.Runtime.Versioning;
using ElkCreekServices.OpenScripts.Logging.Configurations;
using ElkCreekServices.OpenScripts.Logging.Factory;
using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Providers;

/// <summary>
/// A logger provider that creates a rotating file logger
/// </summary>
[UnsupportedOSPlatform("browser")]
[ProviderAlias("RotatingFile")]
public sealed class RotatingFileLoggerProvider : ILoggerProvider
{

    private IDisposable? _onchangeToken;
    private static ConcurrentDictionary<string, Configuration> Configurations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, RotatingFileLoggerFactory> _loggers = new(StringComparer.OrdinalIgnoreCase);

    internal static void AddConfiguration(Func<Configuration> configuration, string? name)
    {
        Configurations.AddOrUpdate(name ?? "default", configuration(), (key, oldValue) => configuration());
    }

    public RotatingFileLoggerProvider()
    {
        
    }

    /// <summary>
    /// Create a logger
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        if (Configurations.ContainsKey(categoryName))
        {
            return new RotatingFileLogger(_loggers.GetOrAdd(categoryName, name => new RotatingFileLoggerFactory(name, () => Configurations[name])));
        }
        else
        {
            return new RotatingFileLogger(_loggers.GetOrAdd("default", name => new RotatingFileLoggerFactory(name, () => Configurations.GetOrAdd(name, name => new Configuration()))));
        }
    }



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
