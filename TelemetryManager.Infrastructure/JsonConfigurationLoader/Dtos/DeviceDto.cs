using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelemetryManager.Infrastructure.JsonConfigurationLoader.Dtos;

public class DeviceDto
{
    [JsonPropertyName("DeviceId")]
    public int DeviceId { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Sensors")]
    public List<SensorDto> Sensors { get; set; } = new List<SensorDto>();
}

public class SensorDto
{
    [JsonPropertyName("TypeId")]
    public byte TypeId { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("SourceId")]
    public int SourceId { get; set; }

    [JsonPropertyName("Parameters")]
    public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();
}

public class ParameterDto
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Units")]
    public string Units { get; set; }

    [JsonPropertyName("Min")]
    public double? Min { get; set; }  // Nullable так как не объязательное поле

    [JsonPropertyName("Max")]
    public double? Max { get; set; }  // Nullable так как не объязательное поле

    public bool HasLimits => Min.HasValue || Max.HasValue;
}