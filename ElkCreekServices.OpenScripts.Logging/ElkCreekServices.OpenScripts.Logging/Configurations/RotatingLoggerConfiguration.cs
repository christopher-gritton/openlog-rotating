﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
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
public class RotatingLoggerConfiguration
{

    private FileInfo? _filename;

    internal delegate void OnFilePathChanged(System.IO.FileInfo filepath);
    internal event OnFilePathChanged? FilePathChanged;


    public RotatingLoggerConfiguration()
    {

    }

    /// <summary>
    /// The name of the logger
    /// </summary>
    public string Name { get; set; } = null!;

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
    /// Generate the directory if it does not exist
    /// </summary>
    public bool AutoGenerateDirectory { get; set; } = false;

    /// <summary>
    /// Attempt to write to a different file if an IOException occurs
    /// </summary>
    public bool AttemptAutoFileRenameOnIOException { get; set; } = false;

    /// <summary>
    /// Name of file if configured
    /// </summary>
    [JsonIgnore]
    [IgnoreDataMember]
    public FileInfo? Filename
    {
        get { return _filename; }
        set { _filename = value; if (FilePathChanged != null) FilePathChanged(value!); }
    }

    [JsonPropertyName("Filename")]
    [DataMember(Name = "Filename")]
    [ConfigurationKeyName("Filename")]
    public string FilenameString
    {
        get { return _filename?.FullName ?? string.Empty; }
        set { _filename = new FileInfo(value); if (FilePathChanged != null) FilePathChanged(_filename); }
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
