namespace TelemetryManager.Application.Interfaces.Services;

public interface IContentDefinitionService
{
    Task RegisterAsync(
        byte id,
        string contentName,
        IEnumerable<(string ParamName, string Description, string Unit, Type DataType)> parameters);
}
