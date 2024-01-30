using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using ElkCreekServices.OpenScripts.Logging;

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
