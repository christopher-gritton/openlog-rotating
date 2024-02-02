using Microsoft.Extensions.Logging;

namespace ElkCreekServices.OpenScripts.Logging.Scope;
public interface IScopedLogger : ILogger, IDisposable
{
    string ScopeId { get; }
}
