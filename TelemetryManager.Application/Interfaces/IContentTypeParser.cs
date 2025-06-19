namespace TelemetryManager.Application.Interfaces;

public interface IContentTypeParser
{
    IReadOnlyDictionary<string, object> ParseRaw(byte typeId, byte[] data);

    IReadOnlyDictionary<string, double> Parse(byte typeId, byte[] data);
}
