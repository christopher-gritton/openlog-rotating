namespace ElkCreekServices.OpenScripts.Logging.Configurations;
public interface IRotatingLoggingConfigurations
{
    /// <summary>
    /// Add a new logging configuration to the collection
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="overwriteOptions"></param>
    void Add(RotatingLoggerConfiguration configuration, OverwriteOptions overwriteOptions = OverwriteOptions.None);

    /// <summary>
    /// Add a new logging configuration to the collection
    /// Only updates the properties listed in the parameters if exists already
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="propertiesToUpdateOnExisting">If exists update existing with only properties listed as parameters</param>
    void Add(RotatingLoggerConfiguration configuration, params string[] propertiesToUpdateOnExisting);
    /// <summary>
    /// Update prarameters of an existing configuration
    /// </summary>
    /// <param name="name">if null or empty then default is assumed</param>
    /// <param name="configure">The action to execute with the matching configuration</param>
    void Override(string name, Action<RotatingLoggerConfiguration> configure);
}
