﻿// See https://aka.ms/new-console-template for more information

#define RUNTOP

using ElkCreekServices.OpenScripts.Logging.Configurations;
using Microsoft.Extensions.Logging;

#if RUNTOP
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif

#if !RUNTOP
using ElkCreekServices.OpenScripts.Logging.Factory;
#endif



namespace ElkCreekServices.OpenScripts.Logging.Example;




class Program
{
    static void Main(string[] args)
    {

#if RUNTOP

        IHostBuilder host = Host.CreateDefaultBuilder(args);

        host.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        });

        host.ConfigureServices((context, services) =>
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders()
                //.AddFilter("Microsoft", LogLevel.Warning)
                //.AddFilter("System", LogLevel.Warning)
                //.AddFilter("ElkCreekServices.OpenScripts.Logging.Example", LogLevel.Trace)
                //.AddRotatingFileLogger();
                .AddRotatingFileLogger(config =>
                {

                    config.Add(new RotatingLoggerConfiguration()
                    {
                        LogLevel = LogLevel.Trace,
                        ConsoleLoggingEnabled = false,
                        ConsoleMinLevel = LogLevel.Trace,
                        Filename = new System.IO.FileInfo("./logging/log_default.txt"),
                        IncludeDateTime = true,
                        IsUtcTime = true,
                        PurgeAfterDays = 2,
                        AutoGenerateDirectory = true,
                        AttemptAutoFileRenameOnIOException = true,
                        MaximumLogFileSizeKB = (1024 * 1024 * 10)
                    }, OverwriteOptions.UpdateIfNull);

                    ////add a new configuration for the logger
                    //config.Add(new RotatingLoggerConfiguration()
                    //{
                    //    LogLevel = LogLevel.Trace,
                    //    ConsoleLoggingEnabled = false,
                    //    ConsoleMinLevel = LogLevel.Trace,
                    //    Filename = new System.IO.FileInfo("./logging/log_default.txt"),
                    //    IncludeDateTime = true,
                    //    IsUtcTime = true,
                    //    PurgeAfterDays = 2,
                    //    AutoGenerateDirectory = true,
                    //    AttemptAutoFileRenameOnIOException = true,
                    //    MaximumLogFileSizeKB = (1024 * 1024 * 10)
                    //}, false, [nameof(RotatingLoggerConfiguration.ConsoleMinLevel),
                    //        nameof(RotatingLoggerConfiguration.ConsoleLoggingEnabled),
                    //        nameof(RotatingLoggerConfiguration.MaximumLogFileSizeKB)]);

                    //config.Add(new RotatingLoggerConfiguration()
                    //{
                    //    Name = "program",
                    //    LogLevel = LogLevel.Trace,
                    //    ConsoleLoggingEnabled = false,
                    //    ConsoleMinLevel = LogLevel.Trace,
                    //    Filename = new System.IO.FileInfo("./logging/log_program.txt"),
                    //    IncludeDateTime = true,
                    //    IsUtcTime = true,
                    //    PurgeAfterDays = 2,
                    //    AutoGenerateDirectory = true,
                    //    AttemptAutoFileRenameOnIOException = true,
                    //    MaximumLogFileSizeKB = 5 //(1024 * 1024 * 10)
                    //}, OverwriteOptions.Overwrite);

                    //config.Add(new RotatingLoggerConfiguration()
                    //{
                    //    Name = "custom",
                    //    LogLevel = LogLevel.Trace,
                    //    ConsoleLoggingEnabled = true,
                    //    ConsoleMinLevel = LogLevel.Trace,
                    //    Filename = new System.IO.FileInfo("log_custom.txt"),
                    //    IncludeDateTime = true,
                    //    IsUtcTime = true,
                    //    PurgeAfterDays = 2,

                    //});

                    //config.Add(new RotatingLoggerConfiguration()
                    //{
                    //    Name = "chat",
                    //    LogLevel = LogLevel.Trace,
                    //    ConsoleLoggingEnabled = true,
                    //    ConsoleMinLevel = LogLevel.Trace,
                    //    Filename = new System.IO.FileInfo("./logging/log_default.txt"),
                    //    IncludeDateTime = true,
                    //    IsUtcTime = true,
                    //    PurgeAfterDays = 2,
                    //    AutoGenerateDirectory = true,
                    //    AttemptAutoFileRenameOnIOException = true
                    //});

                    //update parameters on existing configuration
                    //config.Override("chat", configure =>
                    //{
                    //    configure.LogLevel = LogLevel.Debug;
                    //    configure.Filename = new System.IO.FileInfo("./logging/log_chat2.txt");
                    //});
                });
            });


        });

        using (IHost hostInstance = host.Build())
        {

            //create loggers from factory
            ILogger? logger = hostInstance.Services.GetService<ILoggerFactory>()?.CreateLogger<Program>();
            ILogger? logger2 = hostInstance.Services.GetService<ILoggerFactory>()?.CreateLogger<Chat>();
            //create logger from provider
            ILogger? logger3 = hostInstance.Services.GetService<ILoggerProvider>()?.CreateLogger("custom");

            try
            {
                throw new Exception("This is a test exception");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred in the main method");
            }


            logger?.LogInformation("Example log message");
            logger2?.LogInformation("Example log message 2");
            logger3?.LogInformation("Example log message 3");

            logger?.i("Informational message");
            logger?.d("Debug message");
            logger?.w("Warning message");

            using (logger?.BeginScope("scopeId"))
            {
                logger?.LogInformation("Example scoped log message");
                logger?.LogCritical("Critical message");
                logger?.LogDebug("Debug message");
                logger?.LogTrace("Trace message");
                logger?.LogError("Error message");
                using (logger?.BeginScope("nestedScopeId"))
                {
                    logger?.LogInformation("Example nested scoped log message");
                    using (logger?.BeginScope("nestedScopeId2"))
                    {
                        logger?.LogWarning("Example nested scoped log warning");
                    }
                }
            }

            List<Task> tasks = new List<Task>();

            Action<int> taskAction = (i) =>
            {
                using (logger?.BeginScope("running-bg-task-outer"))
                {
                    /* 
                      The logger is thread safe, so we can use it in a multi-threaded environment.
                      We can also use the BeginScope method to add a scope and use the extension method that
                      returns an IScopedLogger which we can use to log with a unique id for the scope. This is very useful
                      in multi threaded environments where we want to track the logs for a specific task.
                    */

                    using (var scoped = logger?.BeginScope("running-bg-task", Guid.NewGuid().ToString()))
                    {
                        try
                        {
                            logger?.LogDebug("Debug logging on default");
                            logger?.LogInformation("Example log message");
                            logger2?.LogInformation("Example log message 2");
                            logger3?.LogInformation("Example log message 3");
                            string _taskId = "task" + i;

                            scoped?.LogInformation("Task " + i + " is running");
                            scoped?.LogWarning("The task is going to sleep for 1 second");
                            System.Threading.Thread.Sleep(1000);
                            if (i % 7 == 0) { throw new Exception("Task " + i + " failed"); }
                            if (i % 25 == 0) { throw new ArgumentOutOfRangeException("Task " + i + " is out of range"); }
                            scoped?.LogInformation("Task " + i + " is complete");
                        }
                        catch (ArgumentOutOfRangeException aorex)
                        {
                            scoped?.LogCritical(aorex, "A critical error has occurred");
                        }
                        catch (Exception ex)
                        {
                            scoped?.LogError(ex, "An error occurred in the background task");
                        }
                    }
                }
            };

            using (logger?.BeginScope("background-tasks"))
            {
                for (int i = 0; i < 500; i++)
                {

                    int index = i;
                    logger?.LogInformation("Starting task " + i);
                    tasks.Add(Task.Run(() => taskAction(index)));
                    Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
                }

                Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult();
            }

            //give enough time for logger to finish writing to console for clean example screenshot
            Task.Delay(2000).GetAwaiter().GetResult();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();




        }

#endif

#if !RUNTOP
            using (RotatingFileLoggerFactory logger = new RotatingFileLoggerFactory(string.Empty, () => new RotatingLoggerConfiguration()
            {
                LogLevel = LogLevel.Trace,
                Filename = new System.IO.FileInfo("./logging/log_example.txt"),
            }))
            {
                try
                {
                    throw new Exception("This is a test exception");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred in the main method");
                }

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

#endif

    }

}

