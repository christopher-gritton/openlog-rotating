namespace ElkCreekServices.OpenScripts.Logging.Configurations;

/// <summary>
/// Overwrite options for adding a new configuration
/// </summary>
public enum OverwriteOptions
{
    /// <summary>
    /// Just ignore if exists
    /// </summary>
    None,
    /// <summary>
    /// Overwrite if exists
    /// </summary>
    Overwrite,
    /// <summary>
    /// Update if exists
    /// </summary>
    Update,
    /// <summary>
    /// Update if null
    /// </summary>
    UpdateIfNull,
    /// <summary>
    /// Throw an exception if exists
    /// </summary>
    Throw
}
