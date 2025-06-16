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

    public DeviceProfile Load(string configFilePath)
    {
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException("Configuration file not found", configFilePath);

        var json = File.ReadAllText(configFilePath);

        DeviceProfile? profile;

        try
        {
            profile = JsonSerializer.Deserialize<DeviceProfile>(json, _options);
        }
        catch (JsonException ex) 
        {
            throw new InvalidDataException("Invalid JSON format in configuration file", ex);
        }
        if (profile == null)
            throw new InvalidDataException("Invalid or empty configuration");

        return profile;
    }

}
