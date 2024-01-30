namespace ElkCreekServices.OpenScripts.Logging.Scope;

internal class ScopedLogger : IDisposable
{

    public ScopedLogger(string scopeId)
    {
        Id = scopeId;
    }

    public string Id { get; }

    public event EventHandler<bool>? Disposing;

    public void Dispose()
    {
        //if any cleanup is needed
        Disposing?.Invoke(this, true);
    }
}
