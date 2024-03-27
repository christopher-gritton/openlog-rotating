using ElkCreekServices.OpenScripts.Logging.Factory;
using ElkCreekServices.OpenScripts.Logging.Scope;
using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging;
internal sealed class RotatingFileLogger : IScopedLogger
{

    // list of scoped loggers
    private List<ScopedLogger> _scopedLoggers = new List<ScopedLogger>();
    // flag: is disposed
    private bool _disposedValue;
    // scope lock
    private object scopeLock = new { scopelock = "locked" };

    private RotatingFileLoggerFactory _factory;

    public string ScopeId { get; private set; } = string.Empty;

    public RotatingFileLogger(RotatingFileLoggerFactory factory)
    {
        _factory = factory;
    }


    /*
     Default beginscope implementation
     */
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return BeginScope<TState>(state, string.Empty);
    }

    /// <summary>
    /// Create a scoped logger
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <param name="scopeid"></param>
    /// <returns></returns>
    public IDisposable? BeginScope<TState>(TState state, string scopeid) where TState : notnull
    {
        //This scope object doesn't need an external scope so setting it to null
        var newScope = new ScopedLogger(state.ToString() ?? Guid.NewGuid().ToString(), this, null, scopeid);
        lock (scopeLock)
        {
            this._scopedLoggers.Add(newScope);
        }

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
    public bool IsEnabled(LogLevel logLevel) => _factory.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        //if the log level is not enabled, return
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string scopeEntry = string.Empty;
        //log the message with scopes if exist
        lock (scopeLock)
        {
             scopeEntry = string.Join(":", _scopedLoggers.Select(x => x.Id).Distinct());
        }

        // get the formatted message
        var message = $"{(string.IsNullOrWhiteSpace(scopeEntry) ? string.Empty : "[" + scopeEntry + "] ")}{formatter(state, exception)}";

        //microsoft formatter just returns the state value so we will include our own formatter 
        _factory.Log<string>(logLevel, eventId, message, exception, (s, e) => { return s + (e != null ? Environment.NewLine + "Exception: " + e.Message + "\n" + e.StackTrace : string.Empty); });
    }

   

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {        
            //dispose scopes
            if (disposing)
            {
                lock (scopeLock)
                {
                    foreach (var scope in _scopedLoggers)
                    {
                        scope.Dispose();
                    }
                    _scopedLoggers.Clear();
                }
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
