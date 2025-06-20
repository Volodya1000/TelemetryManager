using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Application.ContentTypeProcessing;

public class ContentTypeParser : IContentTypeParser
{
    private readonly IContentDefinitionRepository _repository;

    public ContentTypeParser(IContentDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyDictionary<string, object>> ParseRawAsync(byte typeId, byte[] data)
    {
        var def = await _repository.GetDefinitionAsync(typeId);

        if (data.Length < def.TotalSizeBytes)
            throw new ArgumentException($"Требуется {def.TotalSizeBytes} байт, получено {data.Length}");

        var result = new Dictionary<string, object>(def.Parameters.Count);
        int offset = 0;

        foreach (var parameter in def.Parameters)
        {
            var span = new ReadOnlySpan<byte>(data, offset, parameter.ByteSize);
            offset += parameter.ByteSize;
            result[parameter.Name.Value] = parameter.Handler.ParseValue(span);
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<string, double>> ParseAsync(byte typeId, byte[] data)
    {
        var rawValues = await ParseRawAsync(typeId, data);
        var def = await _repository.GetDefinitionAsync(typeId);

        return def.Parameters
            .ToDictionary(
                parameter => parameter.Name.Value,
                parameter => parameter.Handler.ConvertToDouble(rawValues[parameter.Name.Value])
            );
    }
}