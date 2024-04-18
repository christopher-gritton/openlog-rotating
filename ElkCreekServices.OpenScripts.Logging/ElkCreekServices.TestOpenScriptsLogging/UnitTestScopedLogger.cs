using ElkCreekServices.OpenScripts.Logging;
using ElkCreekServices.OpenScripts.Logging.Configurations;
using Microsoft.Extensions.Logging;

namespace TestOpenScriptsLogging;

[TestClass]
[TestCategory("Logging:Scoped")]
public class UnitTestScopedLogger
{
    private ILoggerFactory? _loggerFactory;

    [TestInitialize]
    public void Initialize()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("TestOpenScriptsLogging", LogLevel.Debug)
                .AddRotatingFileLogger(config =>
                    config.Add(new RotatingLoggerConfiguration()
                    {
                        LogLevel = LogLevel.Debug,
                        ConsoleLoggingEnabled = true,
                        ConsoleMinLevel = LogLevel.Debug,
                        Filename = new System.IO.FileInfo("log_scoped_multilogger.txt"),
                        IncludeDateTime = true,
                        IsUtcTime = true,
                        PurgeAfterDays = 2,
                    }));
        });
       
    }

    [TestCleanup]
    public void Cleanup()
    {
        _loggerFactory?.Dispose();
    }

    [TestMethod]
    public void AssertCanInstantiate()
    {
        var _logger = _loggerFactory!.CreateLogger<UnitTestScopedLogger>();
        using (var scopedLogger = _logger!.BeginScope("AssertCanInstantiate"))
        {
            Assert.IsNotNull(scopedLogger);
            _logger.LogInformation("Example scoped log message");
        }
    }

}
