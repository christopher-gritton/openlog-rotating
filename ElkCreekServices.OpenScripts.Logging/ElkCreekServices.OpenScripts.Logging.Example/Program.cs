﻿// See https://aka.ms/new-console-template for more information

using ElkCreekServices.OpenScripts.Logging.Factory;
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
                 .AddRotatingFileLogger(() => new Configurations.Configuration()
                 {
                     LogLevel = LogLevel.Trace,
                     ConsoleLoggingEnabled = true,
                     ConsoleMinLevel = LogLevel.Trace,
                     Filename = new System.IO.FileInfo("./logging/log_default.txt"),
                     IncludeDateTime = true,
                     IsUtcTime = true,
                     PurgeAfterDays = 2,
                     AutoGenerateDirectory = true,
                     AttemptAutoFileRenameOnIOException = true,
                     MaximumLogFileSizeKB = (1024 * 1024 * 10)
                 })
                  .AddRotatingFileLogger(() => new Configurations.Configuration()
                  {
                      LogLevel = LogLevel.Trace,
                      ConsoleLoggingEnabled = true,
                      ConsoleMinLevel = LogLevel.Trace,
                      Filename = new System.IO.FileInfo("./logging/log_default.txt"),
                      IncludeDateTime = true,
                      IsUtcTime = true,
                      PurgeAfterDays = 2,
                      AutoGenerateDirectory = true,
                      AttemptAutoFileRenameOnIOException = true
                  }, "chat")
                .AddRotatingFileLogger(() => new Configurations.Configuration()
                {
                    LogLevel = LogLevel.Trace,
                    ConsoleLoggingEnabled = true,
                    ConsoleMinLevel = LogLevel.Trace,
                    Filename = new System.IO.FileInfo("log_custom.txt"),
                    IncludeDateTime = true,
                    IsUtcTime = true,
                    PurgeAfterDays = 2,
                }, "custom");
        });

       
        ILogger logger = loggerFactory.CreateLogger<Program>();
        ILogger logger2 = loggerFactory.CreateLogger("chat");
        ILogger logger3  = loggerFactory.CreateLogger("custom");

        logger.LogInformation("Example log message");
        logger2.LogInformation("Example log message 2");
        logger3.LogInformation("Example log message 3");

        logger.i("Informational message");
        logger.d("Debug message");
        logger.w("Warning message");

        using (logger.BeginScope("scopeId"))
        {
            logger.LogInformation("Example scoped log message");
            logger.LogCritical("Critical message");
            logger.LogDebug("Debug message");
            logger.LogTrace("Trace message");
            logger.LogError("Error message");
            using (logger.BeginScope("nestedScopeId"))
            {
                logger.LogInformation("Example nested scoped log message");
                using (logger.BeginScope("nestedScopeId2"))
                {
                    logger.LogWarning("Example nested scoped log warning");
                }
            }
        }

        List<Task> tasks = new List<Task>();

        Action<int> taskAction = (i) =>
        {
            using (logger.BeginScope("running-bg-task-outer"))
            {
                /* 
                  The logger is thread safe, so we can use it in a multi-threaded environment.
                  We can also use the BeginScope method to add a scope and use the extension method that
                  returns an IScopedLogger which we can use to log with a unique id for the scope. This is very useful
                  in multi threaded environments where we want to track the logs for a specific task.
                */

                using (var scoped = logger.BeginScope("running-bg-task", Guid.NewGuid().ToString()))
                {
                    try
                    {
                        logger.LogInformation("Example log message");
                        logger2.LogInformation("Example log message 2");
                        logger3.LogInformation("Example log message 3");
                        string _taskId = "task" + i;

                        scoped.LogInformation("Task " + i + " is running");
                        scoped.LogWarning("The task is going to sleep for 1 second");
                        System.Threading.Thread.Sleep(1000);
                        if (i % 7 == 0) { throw new Exception("Task " + i + " failed"); }
                        if (i % 25 == 0) { throw new ArgumentOutOfRangeException("Task " + i + " is out of range"); }
                        scoped.LogInformation("Task " + i + " is complete");
                    }
                    catch (ArgumentOutOfRangeException aorex)
                    {
                        scoped.LogCritical(aorex, "A critical error has occurred");
                    }
                    catch (Exception ex)
                    {
                        scoped.LogError(ex, "An error occurred in the background task");
                    }
                }
            }
        };

        using (logger.BeginScope("background-tasks"))
        {
            for (int i = 0; i < 50; i++)
            {

                int index = i;
                logger.LogInformation("Starting task " + i);
                tasks.Add(Task.Run(() => taskAction(index)));

            }

            Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult();
        }

        //give enough time for logger to finish writing to console for clean example screenshot
        Task.Delay(2000).GetAwaiter().GetResult();

        Console.WriteLine("Press enter to exit");
        Console.ReadLine();

        initInternally();

        Console.WriteLine("Press enter to exit");
        Console.ReadLine();

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
            ConsoleMinLevel = LogLevel.Trace,
            Filename = new System.IO.FileInfo(loggingConfig.GetValue("Filename", "internal_log.txt")!),
            IncludeDateTime = bool.Parse(loggingConfig.GetValue("IncludeDateTime", "true")!),
            IsUtcTime = bool.Parse(loggingConfig.GetValue("IsUtcTime", "true")!),
            PurgeAfterDays = int.Parse(loggingConfig.GetValue("PurgeAfterDays", "2")!),
            MaximumLogFileSizeKB = int.Parse(loggingConfig.GetValue("MaximumLogFileSizeKB", "1024")!)
        };

        using RotatingFileLoggerFactory logger = new RotatingFileLoggerFactory("Example Logger", () =>
        {
            return rotatingConfiguration;
        });

        logger.LogInformation("Example log message"); //extension methods for log levels

        using (logger.BeginScope("scopeId"))
        {
            logger.LogInformation("Example scoped log message");
            using (logger.BeginScope("nestedScopeId"))
            {
                logger.LogInformation("Example nested scoped log message");
                logger.LogWarning("Example nested scoped log warning");
            }
        }

        //non - extension methods for log levels
        logger.Log(LogLevel.Information, new EventId(0), "Example log message without extension methods");

    }

}

