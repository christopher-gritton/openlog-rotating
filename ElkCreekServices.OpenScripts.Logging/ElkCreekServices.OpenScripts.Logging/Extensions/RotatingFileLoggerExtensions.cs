using ElkCreekServices.OpenScripts.Logging.Configurations;
using ElkCreekServices.OpenScripts.Logging.Providers;
using ElkCreekServices.OpenScripts.Logging.Scope;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace ElkCreekServices.OpenScripts.Logging;

/// <summary>
/// Extension methods for the RotatingFileLogger
/// </summary>
public static class RotatingFileLoggerExtensions
{

    /// <summary>
    /// Adds a rotating file logger to the logging builder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddRotatingFileLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RotatingFileLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<RotatingLoggerConfigurations, RotatingFileLoggerProvider>(builder.Services);
        return builder;
    }

    /// <summary>
    /// Adds a rotating file logger to the logging builder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ILoggingBuilder AddRotatingFileLogger(this ILoggingBuilder builder, Action<IRotatingLoggingConfigurations> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        builder.AddRotatingFileLogger();      
        builder.Services.Configure<RotatingLoggerConfigurations>(config => configure(config));
        return builder;
    }

    /// <summary>
    /// Creates a scoped instance of the logger
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logger"></param>
    /// <param name="state"></param>
    /// <param name="scopeid"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IScopedLogger BeginScope<TState>(this ILogger logger, TState state, string scopeid) where TState : notnull
    {
        if (logger == null) throw new ArgumentNullException("logger", "The logger cannot be null when calling BeginScope.");
        IScopedLogger? scopedLogger = null;
        IDisposable? externalscope = logger.BeginScope(state);
        scopedLogger = new ScopedLogger(state?.ToString() ?? Guid.NewGuid().ToString(), logger, externalscope, scopeid);
        return scopedLogger;
    }

    /// <summary>
    /// Shortcut to log a debug message
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    public static void d(this ILogger logger, string message)
    {
        logger.LogDebug(message);
    }

    /// <summary>
    /// Shortcut to log an informational message
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    public static void i(this ILogger logger, string message)
    {
        logger.LogInformation(message);
    }

    /// <summary>
    /// Shortcut to log a warning message
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    public static void w(this ILogger logger, string message)
    {
        logger.LogWarning(message);
    }

}
