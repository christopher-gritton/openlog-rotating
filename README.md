
# openlog-rotating  [![Build](https://github.com/christopher-gritton/openlog-rotating/actions/workflows/dotnet.yml/badge.svg)](https://github.com/christopher-gritton/openlog-rotating/actions/workflows/dotnet.yml)
Rotating file logging library for C#

This library is a simple file logging library that supports log rotation. It can be used with or without Microsoft.Extensions.Logging.
The number of days to keep the log files can be configured. The library will automatically delete log files older than the specified number of days.
The maximum file size for a log file before it is rotated can also be configured. The library will automatically rotate the log file when the maximum file size is reached.


## Usage with Microsoft.Extensions.Logging

```csharp

using ElkCreekServices.OpenScripts.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Example;

class Program
{
    static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("OpenScripts.Logging.Example", LogLevel.Warning) // set the log level for this namespace
                .AddRotatingFileLogger((configuration) =>
                {
                    configuration.LogLevel = LogLevel.Debug;
                    configuration.ConsoleLoggingEnabled = true;
                    configuration.Filename = new System.IO.FileInfo("log.txt");
                    configuration.IncludeDateTime = true;
                    configuration.IsUtcTime = true;
                    configuration.PurgeAfterDays = 2;
                });
        });

        // Create a logger
        ILogger logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Example log message");

        using (var scopedLogger = logger.BeginScope("scopeId"))
        {
            logger.LogInformation("Example scoped log message");
            using (var nestedScopedLogger = logger.BeginScope("nestedScopeId"))
            {
                logger.LogInformation("Example nested scoped log message");
                logger.LogWarning("Example nested scoped log warning");
            }
        }
    }
}

```


## Usage without Microsoft.Extensions.Logging.LoggerFactory

`Note: The library will still have dependencies on the Microsoft.Extensions.Logging.Abstractions and Microsoft.Extensions.Logging.Configuration packages`


```csharp

using ElkCreekServices.OpenScripts.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Example;

class Program
{
    static void Main(string[] args)
    {
        using RotatingFileLogger logger = new RotatingFileLogger("Example Logger", () =>
        {
            return new Configurations.Configuration()
            {
                LogLevel = LogLevel.Debug,
                ConsoleLoggingEnabled = true,
                Filename = new System.IO.FileInfo("internal_log.txt"),
                IncludeDateTime = true,
                IsUtcTime = true,
                PurgeAfterDays = 2
            };
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

```

#### Todo 

- Finish unit tests
- Update documentation
