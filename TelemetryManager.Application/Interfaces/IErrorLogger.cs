using TelemetryManager.Core.Data;

namespace TelemetryManager.Application.Interfaces;

public interface IErrorLogger
{
    void LogError(ParsingError error);
    IReadOnlyList<ParsingError> GetErrors();
}