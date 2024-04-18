﻿namespace ElkCreekServices.OpenScripts.Logging.Configurations;

/// <summary>
/// Collection of logging configurations for rotating loggers
/// </summary>
public class RotatingLoggerConfigurations : IRotatingLoggingConfigurations
{

    public IEnumerable<RotatingLoggerConfiguration> Configurations { get; set; } = null!;

    /// <summary>
    /// Add a new logging configuration to the collection
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="overwrite"></param>
    /// <exception cref="Exception"></exception>
    public void Add(RotatingLoggerConfiguration configuration, bool overwrite = false)
    {
        Configurations ??= [];
        if (Configurations.FirstOrDefault(c => (c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true)) != null)
        {
            if (overwrite)
            {
                Configurations = Configurations.Where(c => !(c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true));
                Configurations = Configurations.Concat([configuration]);
            }
            else
            {
                throw new Exception("Logging configuration with the same name already exists");
            }
        }
         else
        {
            Configurations = Configurations.Concat([configuration]);
        }  
       
    }

    /// <summary>
    /// Override parameters on an existing configuration
    /// </summary>
    /// <param name="name"></param>
    /// <param name="configure"></param>
    public void Override(string name, Action<RotatingLoggerConfiguration> configure)
    {
        Configurations ??= [ new RotatingLoggerConfiguration() {  Name = name }];
        RotatingLoggerConfiguration? configuration = Configurations.FirstOrDefault(c => c.Name == name || c.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        if (configuration == null) Configurations = Configurations.Concat([new RotatingLoggerConfiguration() { Name = name }]);
        configure(Configurations.FirstOrDefault(c => c.Name == name || c.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)!);   
    }
}
