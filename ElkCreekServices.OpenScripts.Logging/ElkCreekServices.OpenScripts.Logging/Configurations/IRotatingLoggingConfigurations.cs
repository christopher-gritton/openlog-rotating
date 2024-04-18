namespace ElkCreekServices.OpenScripts.Logging.Configurations;
public interface IRotatingLoggingConfigurations
{
    /// <summary>
    /// Add a new logging configuration to the collection
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="overwrite">Optionally overwrite otherwise an exception is thrown if exists</param>
    void Add(RotatingLoggerConfiguration configuration, bool overwrite = false);
    /// <summary>
    /// Update prarameters of an existing configuration
    /// </summary>
    /// <param name="name">if null or empty then default is assumed</param>
    /// <param name="configure">The action to execute with the matching configuration</param>
    void Override(string name, Action<RotatingLoggerConfiguration> configure);
}
