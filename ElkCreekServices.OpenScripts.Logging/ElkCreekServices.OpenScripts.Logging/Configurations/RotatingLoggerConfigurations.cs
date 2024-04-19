using System.Reflection;

namespace ElkCreekServices.OpenScripts.Logging.Configurations;

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
    public void Add(RotatingLoggerConfiguration configuration, OverwriteOptions overwriteOptions = OverwriteOptions.None)
    {
        Configurations ??= [];
        if (Configurations.FirstOrDefault(c => (c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true)) != null)
        {
            switch (overwriteOptions)
            {
                case OverwriteOptions.None:
                    return;
                case OverwriteOptions.Overwrite:
                    Configurations = Configurations.Where(c => !(c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true));
                    Configurations = Configurations.Concat([configuration]);
                    break;
                case OverwriteOptions.Update:
                    Configurations.FirstOrDefault(c => c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true)?.Update(configuration);
                    break;
                case OverwriteOptions.Throw:
                    throw new Exception("Logging configuration with the same name already exists");
            }
        }
         else
        {
            Configurations = Configurations.Concat([configuration]);
        }  
       
    }

    /// <summary>
    /// Add a new logging configuration to the collection
    /// Always updates the properties listed in the parameters if exists already
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="propertiesToUpdateOnExisting"></param>
    public void Add(RotatingLoggerConfiguration configuration, params string[] propertiesToUpdateOnExisting)
    {
        Configurations ??= [];
        RotatingLoggerConfiguration? existing = Configurations.FirstOrDefault(c => c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true);
        if (existing != null)
        {
            if (propertiesToUpdateOnExisting == null || propertiesToUpdateOnExisting.Length == 0) return;
            foreach (string property in propertiesToUpdateOnExisting)
            {
                PropertyInfo? prop = typeof(RotatingLoggerConfiguration).GetProperty(property);
                prop?.SetValue(existing, prop.GetValue(configuration));
            }
        }
        else
        {
            Configurations = Configurations.Concat([configuration]);
        }
    }

    /// <summary>
    /// Add a new logging configuration to the collection
    /// Updates the properties listed in the parameters if exists already but only if encforceParamProperties is true or the property is null
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="enforceParamProperties"></param>
    /// <param name="propertiesToUpdateOnExisting"></param>
    public void Add(RotatingLoggerConfiguration configuration, bool enforceParamProperties, params string[] propertiesToUpdateOnExisting)
    {
        Configurations ??= [];
        RotatingLoggerConfiguration? existing = Configurations.FirstOrDefault(c => c.Name == configuration.Name || c.Name?.Equals(configuration.Name, StringComparison.OrdinalIgnoreCase) == true);
        if (existing != null)
        {
            if (propertiesToUpdateOnExisting == null || propertiesToUpdateOnExisting.Length == 0) return;
            foreach (string property in propertiesToUpdateOnExisting)
            {
                PropertyInfo? prop = typeof(RotatingLoggerConfiguration).GetProperty(property);
                //only update if enforceParamProperties is true or the property is null
                if (enforceParamProperties || prop?.GetValue(existing) == null) prop?.SetValue(existing, prop.GetValue(configuration));
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
