namespace TelemetryManager.Application.Interfaces;

public interface IContentTypeParser
{
    Task<IReadOnlyDictionary<string, object>> ParseRawAsync(byte typeId, byte[] data);
    Task<IReadOnlyDictionary<string, double>> ParseAsync(byte typeId, byte[] data);
}