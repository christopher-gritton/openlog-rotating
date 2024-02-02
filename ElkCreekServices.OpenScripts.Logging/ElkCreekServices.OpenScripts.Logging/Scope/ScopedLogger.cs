using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Scope;

internal class ScopedLogger : IScopedLogger
{
    public ScopedLogger(string stateId, ILogger host, IDisposable? externalscope, string scopedid = null!)
    {
        ScopeId = scopedid;
        Id = stateId;
        _logger = host;
        _externalScope = externalscope;
    }

    private IDisposable? _externalScope = null;
    private ILogger _logger = null!;

    public string Id { get; }
    public string ScopeId { get; }
    public event EventHandler<bool>? Disposing;

    public void Dispose()
    {
        //if any cleanup is needed
        Disposing?.Invoke(this, true);
        if (_externalScope != null)
            _externalScope.Dispose();
    }

    /*
        Pass the logging information to the host logger.
        If we have a scope id, we will prepend it to the message.
    */
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!string.IsNullOrWhiteSpace(ScopeId))
            _logger.Log(logLevel, eventId, $"[Scope, {ScopeId}] - {formatter(state, exception)}");
        else
        _logger.Log<TState>(logLevel, eventId, state, exception, formatter);
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope<TState>(state);
}
