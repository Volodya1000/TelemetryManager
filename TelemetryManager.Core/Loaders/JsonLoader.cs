using System.Text.Json;
using System.Text.Json.Serialization;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Loaders;

public class JsonLoader : IConfigurationLoader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SensorProfile Load(string configFilePath)
    {
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException("Configuration file not found", configFilePath);

        var json = File.ReadAllText(configFilePath);
        var config = JsonSerializer.Deserialize<SensorProfile>(json, _options);

        if (config == null)
            throw new InvalidDataException("Invalid or empty configuration");

        return config;
    }

}
