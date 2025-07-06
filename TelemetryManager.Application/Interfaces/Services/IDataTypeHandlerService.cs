using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Application.Interfaces.Services;

public interface IDataTypeHandlerService
{
    void RegisterHandler(Type type, IDataTypeHandler handler);
    IDataTypeHandler GetHandler(Type type);
}