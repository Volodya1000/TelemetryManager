using System.Text.Json;
using System.Text.Json.Serialization;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Loaders;

//public class SensorProfileJsonConverter : JsonConverter<SensorProfile>
//{
//    public override SensorProfile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        if (reader.TokenType != JsonTokenType.StartObject)
//            throw new JsonException("Expected start of object");

//        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
//        {
//            var root = doc.RootElement;

//            // Читаем поля из JSON
//            ushort deviceId = root.GetProperty("DeviceId").GetUInt16();
//            byte typeId = root.GetProperty("TypeId").GetByte();
//            byte sourceId = root.GetProperty("SourceId").GetByte();
//            string name = root.GetProperty("Name").GetString()
//                ?? throw new JsonException("Name is required");

//            var parameters = root.TryGetProperty("Parameters", out var parametersEl)
//                ? JsonSerializer.Deserialize<IReadOnlyList<SensorParameterProfile>>(parametersEl, options)
//                : Array.Empty<SensorParameterProfile>();

//            // Собираем SensorId
//            var sensorId = new SensorId(deviceId, typeId, sourceId);

//            // Вызываем ваш конструктор
//            return new SensorProfile(sensorId, name, parameters);
//        }
//    }

//    public override void Write(Utf8JsonWriter writer, SensorProfile value, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException("Serialization not supported");
//    }
//}