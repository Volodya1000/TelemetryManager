using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Data;

namespace TelemetryManager.Application.Logger;

public class MemoryErrorLogger : IErrorLogger
{
    private readonly List<ParsingError> _errors = new();

    public void LogError(ParsingError error)
    {
        _errors.Add(error);
    }

    public IReadOnlyList<ParsingError> GetErrors() => _errors.AsReadOnly();
}