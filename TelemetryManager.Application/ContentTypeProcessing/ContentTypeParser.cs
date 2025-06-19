using TelemetryManager.Application.ContentTypeRegistration;
using TelemetryManager.Application.Interfaces;

namespace TelemetryManager.Application.ContentTypeProcessing;


public class ContentTypeParser
{
    private readonly IContentTypeProvider _registry;

    public ContentTypeParser(IContentTypeProvider registry) => _registry = registry;

    public IReadOnlyDictionary<string, object> ParseRaw(byte typeId, byte[] data)
    {
        var def = _registry.GetDefinition(typeId);
        if (data.Length < def.TotalSizeBytes)
            throw new ArgumentException($"Требуется {def.TotalSizeBytes} байт, получено {data.Length}");

        var res = new Dictionary<string, object>(def.Parameters.Count);
        int offset = 0;

        foreach (var p in def.Parameters)
        {
            var span = new ReadOnlySpan<byte>(data, offset, p.ByteSize);
            offset += p.ByteSize;
            res[p.Name] = p.Handler.ParseValue(span);
        }

        return res;
    }

    public IReadOnlyDictionary<string, double> Parse(byte typeId, byte[] data)
    {
        var rawValues = ParseRaw(typeId, data);
        var def = _registry.GetDefinition(typeId);

        return def.Parameters
            .ToDictionary(
                p => p.Name,
                p => p.Handler.ConvertToDouble(rawValues[p.Name])
            );
    }
}