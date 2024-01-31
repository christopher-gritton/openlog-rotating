// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Example;

class Program
{
    static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .ClearProviders()
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("ElkCreekServices.OpenScripts.Logging.Example", LogLevel.Trace)
                .AddRotatingFileLogger((configuration) =>
                {
                    configuration.LogLevel = LogLevel.Trace;
                    configuration.ConsoleLoggingEnabled = true;
                    configuration.ConsoleMinLevel = LogLevel.Trace;
                    configuration.Filename = new System.IO.FileInfo("log.txt");
                    configuration.IncludeDateTime = true;
                    configuration.IsUtcTime = true;
                    configuration.PurgeAfterDays = 2;
                });
        });

        ILogger logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Example log message");

        using (var scopedLogger = logger.BeginScope("scopeId"))
        {
            logger.LogInformation("Example scoped log message");
            logger.LogCritical("Critical message");
            logger.LogDebug("Debug message");
            logger.LogTrace("Trace message");
            logger.LogError("Error message");
            using (var nestedScopedLogger = logger.BeginScope("nestedScopeId"))
            {
                logger.LogInformation("Example nested scoped log message");
                logger.LogWarning("Example nested scoped log warning");
            }
        }


        Console.WriteLine("Press enter to exit");
        Console.ReadLine();

        initInternally();

    }

    static void initInternally()
    {

        // Read the app settings file, you can add secrets and additional files here
        var builder = new ConfigurationBuilder()
       .SetBasePath(System.IO.Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        // Build the configuration
        var configuration = builder.Build();

        // Load the section for the logger
        var loggingConfig = configuration.GetSection("Logging:RotatingFile");
        //create a new configuration
        var rotatingConfiguration = new Configurations.Configuration()
        {
            LogLevel = loggingConfig.GetValue("LogLevel", LogLevel.Debug)!,
            ConsoleLoggingEnabled = true,
            Filename = new System.IO.FileInfo(loggingConfig.GetValue("Filename", "internal_log.txt")!),
            IncludeDateTime = bool.Parse(loggingConfig.GetValue("IncludeDateTime", "true")!),
            IsUtcTime = bool.Parse(loggingConfig.GetValue("IsUtcTime", "true")!),
            PurgeAfterDays = int.Parse(loggingConfig.GetValue("PurgeAfterDays", "2")!),
            MaximumLogFileSizeKB = int.Parse(loggingConfig.GetValue("MaximumLogFileSizeKB", "1024")!)
        };

        using RotatingFileLogger logger = new RotatingFileLogger("Example Logger", () =>
        {
            return rotatingConfiguration;
        });

        logger.LogInformation("Example log message"); //extension methods for log levels

        using (var scopedLogger = logger.BeginScope("scopeId"))
        {
            logger.LogInformation("Example scoped log message");
            using (var nestedScopedLogger = logger.BeginScope("nestedScopeId"))
            {
                logger.LogInformation("Example nested scoped log message");
                logger.LogWarning("Example nested scoped log warning");
            }
        }

        //non - extension methods for log levels
        logger.Log(LogLevel.Information, new EventId(0), "Example log message without extension methods");

    }

}

