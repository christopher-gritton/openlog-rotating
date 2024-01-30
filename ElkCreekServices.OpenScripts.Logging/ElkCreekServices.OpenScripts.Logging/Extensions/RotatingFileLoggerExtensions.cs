using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using ElkCreekServices.OpenScripts.Logging.Providers;

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
        LoggerProviderOptions.RegisterProviderOptions<Configurations.Configuration, RotatingFileLoggerProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    /// Adds a rotating file logger to the logging builder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ILoggingBuilder AddRotatingFileLogger(this ILoggingBuilder builder, Action<Configurations.Configuration> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.AddRotatingFileLogger();
        builder.Services.Configure(configure);

        return builder;
    }

}
