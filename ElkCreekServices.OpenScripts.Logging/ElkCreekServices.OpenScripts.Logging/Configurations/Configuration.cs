using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Configurations;

/// <summary>
/// Logging configuration
/// </summary>
/// <remarks>
/// Serializable and DataContract are used to allow the class to be serialized and deserialized with either xml or json.
/// </remarks>
[Serializable]
[DataContract]
public class Configuration
{

    private FileInfo? _filename;

    internal delegate void OnFilePathChanged(System.IO.FileInfo filepath);
    internal event OnFilePathChanged? FilePathChanged;

    public Configuration()
    {

    }

    /// <summary>
    /// The logging level
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.None;

    /// <summary>
    /// The minimum level to log to the console
    /// </summary>
    public LogLevel ConsoleMinLevel { get; set; } = LogLevel.Error;

    /// <summary>
    /// If console logging is enabled
    /// </summary>
    public bool ConsoleLoggingEnabled { get; set; } = false;

    /// <summary>
    /// Name of file if configured
    /// </summary>
    public FileInfo? Filename
    {
        get { return _filename; }
        set { _filename = value; if (FilePathChanged != null) FilePathChanged(value!); }
    }

   /// <summary>
   /// Include date/time in log entries
   /// </summary>
   public bool IncludeDateTime { get; set; } = true;

    /// <summary>
    /// Set date/time format to use UTC
    /// </summary>
    public bool IsUtcTime { get; set; } = true;

    /// <summary>
    /// Automatically purge the rotated log file after X days
    /// </summary>
    public int PurgeAfterDays { get; set; } = 90;

    /// <summary>
    /// Maximum log file size in KB before rotating
    /// </summary>
    public int MaximumLogFileSizeKB { get; set; } = 4096;


}

