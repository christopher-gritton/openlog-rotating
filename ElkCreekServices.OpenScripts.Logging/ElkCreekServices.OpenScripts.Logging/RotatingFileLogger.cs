using System.Text;
using Microsoft.Extensions.Logging;
using ElkCreekServices.OpenScripts.Logging.Configurations;
using ElkCreekServices.OpenScripts.Logging.Scope;

namespace ElkCreekServices.OpenScripts.Logging;
public sealed class RotatingFileLogger : ILogger, IDisposable
{

    // list of scoped loggers
    private List<ScopedLogger> _scopedLoggers = new List<ScopedLogger>();
    // flag: is disposed
    private bool _disposedValue;
    // writer lock
    private object writerLock = new { writerlock = "locked" };
    // queue lock
    private object queueLock = new { queuelock = "locked" };
    // scope lock
    private object scopeLock = new { scopelock = "locked" };
    // cancellation token source
    private CancellationTokenSource _cts = new CancellationTokenSource();
    // queue of log entries
    private Queue<QueuedLogEntry> _queued = new Queue<QueuedLogEntry>();
    // Thread for processing log entries
    System.Threading.Thread? _watchlog;
    //streamwriter
    StreamWriter? _sw;
    private readonly string _name;
    private readonly Configuration _configuration;

    // default configuration in case one isn't provided
    public static Configurations.Configuration DefaultConfiguration { get; } = new Configurations.Configuration();

    public RotatingFileLogger(string name, Func<Configurations.Configuration> getConfiguration)
    {
        _name = name;
        _configuration = getConfiguration();
        //start the log watcher
        _watchlog = new System.Threading.Thread((token) => WatchLogging((CancellationToken)token!));
        _watchlog.IsBackground = false;
        _watchlog.Priority = ThreadPriority.Lowest;
        _watchlog.Start(_cts.Token);
    }


    /// <summary>
    /// Create a new scoped logger
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        //create a new scope
        var newScope = new ScopedLogger(state.ToString() ?? Guid.NewGuid().ToString());
        this._scopedLoggers.Add(newScope);
        //remove from scoped loggers when disposed
        newScope.Disposing += (sender, disposing) =>
        {
            if (disposing)
            {
                var scoped = sender as ScopedLogger;
                if (scoped != null)
                {
                    lock (scopeLock)
                    {
                        this._scopedLoggers.Remove(scoped);
                    }
                }
            }
        };
        return newScope;
    }

    /// <summary>
    /// Check if log level is enabled
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel) {
        if (logLevel >= (_configuration ?? DefaultConfiguration).LogLevel && logLevel != LogLevel.None)
        {
            return true;
        }
        return false;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var config = _configuration ?? DefaultConfiguration;

        //if the log level is not enabled, return
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string scopeEntry = string.Empty;
        //log the message with scopes if exist
        lock (scopeLock)
        {
             scopeEntry = string.Join(":", _scopedLoggers.Select(x => x.Id));
        }

        // get the formatted message
        var message = formatter(state, exception);

        //create a log entry
        var queuedEntry = new QueuedLogEntry()
        {
            Message = $"[{eventId}, {logLevel.ToString()}] {(string.IsNullOrWhiteSpace(scopeEntry) ? string.Empty : "[" + scopeEntry + "] ")}{message}",
            IncludeDateTime = config.IncludeDateTime,
            IsUtcTime = config.IsUtcTime,
            LogLevel = logLevel
        };

        //queue the log entry
        lock (queueLock)
        {
            _queued.Enqueue(queuedEntry);
        }

    }

    /// <summary>
    /// Close the streamwriter
    /// </summary>
    private void CloseWriter()
    {
        lock (writerLock)
        {
            if (_sw != null)
            {
                _sw.Dispose();
            }
        }
    }

    /// <summary>
    /// Initialize the streamwriter
    /// </summary>
    /// <remarks>
    /// We maintain an open streamwriter as long as possible to avoid the overhead of opening and closing the file for each log entry.
    /// </remarks>
    private void InitWriter()
    {
        var config = _configuration ?? DefaultConfiguration;
        lock (writerLock)
        {
            if (config.Filename != null)
            {
                if (_sw != null)
                {
                    _sw.Dispose();
                }
                _sw = new StreamWriter(config.Filename.FullName, true, Encoding.UTF8, 65536) { AutoFlush = true };
            }
        }
    }

    private void WatchLogging(CancellationToken token)
    {
        //initialize the writer
        InitWriter();

        _configuration.FilePathChanged += (model) =>
        {
            //we need to initialize the writer again
            InitWriter();
        };

        do
        {
            try
            {
                if (_queued.Count > 0)
                {
                    //make sure log file assigned
                    if (_configuration.Filename == null) break;

                    QueuedLogEntry e;
                    lock (_queued)
                    {
                        e = _queued.Peek(); // get the next log entry but don't dequeue it yet
                    }

                    if (e != null)
                    {
                        //get logging directory and open log file
                        System.Text.StringBuilder sb = new StringBuilder();

                        //write log entry
                        sb.Append(e.ToString());
                        //if configuration includes write to console and console is enabled, write to console
                        if (_configuration.ConsoleLoggingEnabled == true && Environment.UserInteractive)
                        {
                            //only write to console if log level is enabled
                            if (e.LogLevel != LogLevel.None && e.LogLevel >= _configuration.ConsoleMinLevel)
                            {
                                var color = Console.ForegroundColor;
                                Console.ForegroundColor = e.LogLevel switch
                                {
                                    LogLevel.Trace => ConsoleColor.DarkGray,
                                    LogLevel.Debug => ConsoleColor.Gray,
                                    LogLevel.Information => ConsoleColor.Cyan,
                                    LogLevel.Warning => ConsoleColor.DarkYellow,
                                    LogLevel.Error => ConsoleColor.DarkRed,
                                    LogLevel.Critical => ConsoleColor.Red,
                                    _ => ConsoleColor.White
                                };
                                Console.WriteLine(sb.ToString().TrimEnd());
                                Console.ForegroundColor = color;
                            }
                        }

                        try
                        {
                            //get the log file
                            System.IO.FileInfo logfile = new System.IO.FileInfo(_configuration.Filename.FullName);
                            //rotate log files
                            if (logfile.Exists)
                            {
                                //check if log file is too large
                                if (logfile.Length > (1024 * _configuration.MaximumLogFileSizeKB))
                                {
                                    //rotate the log file out
                                    CloseWriter();
                                    logfile.MoveTo($"{logfile.FullName.Replace(logfile.Extension, "")}_[ROT-{DateTime.UtcNow.ToOADate().ToString()}]{logfile.Extension}");
                                    logfile = new System.IO.FileInfo(_configuration.Filename.FullName);
                                    InitWriter();
                                }
                            }
                            //only purge rotated files if setting above 0
                            if (_configuration.PurgeAfterDays > 0)
                            {
                                if (logfile.Directory != null)
                                {
                                    //purge rotated logfiles / use enumeratefiles to get rotated log files will be more efficient than using getfiles
                                    foreach (var d in logfile.Directory.EnumerateFiles($"*{logfile.Name.Replace(logfile.Extension, "")}_[ROT-*", System.IO.SearchOption.TopDirectoryOnly))
                                    {
                                        //check if rotated log file is older than the purge setting
                                        if (DateTime.Now.Subtract(d.LastWriteTime).TotalDays > _configuration.PurgeAfterDays)
                                        {
                                            //delete the file
                                            d.Delete();
                                        }
                                    }
                                }
                            }

                            lock (writerLock)
                            {
                                //write the log entry
                                _sw!.WriteLine(sb.ToString().TrimEnd());
                            }

                            lock (_queued)
                            {
                                //log item should have been written to file so dequeue it
                                QueuedLogEntry d = _queued.Dequeue();
                                if (d != e) throw new ApplicationException("Logging queue has encountered a hard error.");
                            }
                        }
                        catch (System.IO.IOException ioex)
                        {
                            //possible log file access issue
                            System.Diagnostics.Trace.TraceError(ioex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //log to our trace logger 
                System.Diagnostics.Trace.TraceError(ex.ToString());
                if (token.IsCancellationRequested) break;
            }

            //check if we should exit
            if (token.IsCancellationRequested && _queued.Count <= 0)
            {
                break;
            }

            //keep CPU cycles down
            if (_queued.Count == 0) { System.Threading.Thread.Sleep(100); }

        } while (true);

        //close the writer
        if (_sw != null) _sw.Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            //cancel the token
            if (_cts != null && _cts.Token.CanBeCanceled)
            {
                try
                {
                    _cts.Cancel(false);
                }
                catch(Exception ex) {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                }
            }
            //join the background thread
            if (_watchlog != null)
            {
                //timeout after 5 minutes
                _watchlog.Join(300000);
            }
            //dispose the streamwriter
            if (_sw != null)
            {
                try
                {
                    _sw.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                }
            }
            //dispose scopes
            if (disposing)
            {
                foreach (var scope in _scopedLoggers)
                {
                    scope.Dispose();
                }
                _scopedLoggers.Clear();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
