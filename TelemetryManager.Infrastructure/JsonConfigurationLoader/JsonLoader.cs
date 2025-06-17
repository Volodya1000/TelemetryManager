using TelemetryManager.Core.Data.Profiles;
using System.Text.Json;
using System.Text.Json.Serialization;
using TelemetryManager.Infrastructure.JsonConfigurationLoader.Dtos;
using TelemetryManager.Application.Interfaces;

namespace TelemetryManager.Infrastructure.JsonConfigurationLoader;

public class JsonLoader : IConfigurationLoader
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<DeviceProfile> Load(string configFilePath)
    {
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException($"File {configFilePath} does not exist.", configFilePath);

        var json = File.ReadAllText(configFilePath);
        List<DeviceDto>? devicesDto;

        try
        {
            devicesDto = JsonSerializer.Deserialize<List<DeviceDto>>(json, _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Invalid JSON format in configuration file", ex);
        }

        if (devicesDto == null || devicesDto.Count == 0)
            throw new InvalidDataException("Invalid or empty configuration");

        return MapToDomainModels(devicesDto);
    }

    private List<DeviceProfile> MapToDomainModels(List<DeviceDto> devicesDto)
    {
        var deviceProfiles = new List<DeviceProfile>();

        foreach (var deviceDto in devicesDto)
        {
            // Проверка DeviceId на диапазон ushort
            if (deviceDto.DeviceId < ushort.MinValue || deviceDto.DeviceId > ushort.MaxValue)
            {
                throw new InvalidDataException(
                    $"Device ID {deviceDto.DeviceId} is out of range for ushort (0-65535)");
            }

            var sensorProfiles = new List<SensorProfile>();

            foreach (var sensorDto in deviceDto.Sensors)
            {
                // Проверка SourceId на диапазон byte
                if (sensorDto.SourceId < byte.MinValue || sensorDto.SourceId > byte.MaxValue)
                {
                    throw new InvalidDataException(
                        $"Source ID {sensorDto.SourceId} is out of range for byte (0-255)");
                }

                var parameterProfiles = MapParameters(sensorDto.Parameters);

                var sensorProfile = new SensorProfile(
                    TypeId: sensorDto.TypeId,
                    SourceId: (byte)sensorDto.SourceId,
                    Name: sensorDto.Name,
                    Parameters: parameterProfiles
                );

                sensorProfiles.Add(sensorProfile);
            }

            deviceProfiles.Add(new DeviceProfile(
                deviceId: (ushort)deviceDto.DeviceId,
                name: deviceDto.Name,
                sensors: sensorProfiles
            ));
        }

        return deviceProfiles;
    }

    private List<SensorParameterProfile> MapParameters(List<ParameterDto> parametersDto)
    {
        var parameters = new List<SensorParameterProfile>();

        foreach (var paramDto in parametersDto)
        {
            // Обработка отсутствующих значений min/max
            if (!paramDto.Min.HasValue || !paramDto.Max.HasValue)
            {
                throw new InvalidDataException(
                    $"Parameter '{paramDto.Name}' must have both min and max values");
            }

            // Преобразование int? в double
            double min = (double)paramDto.Min.Value;
            double max = (double)paramDto.Max.Value;

            try
            {
                parameters.Add(new SensorParameterProfile(
                    name: paramDto.Name,
                    units: paramDto.Units,
                    min: min,
                    max: max
                ));
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException(
                    $"Invalid parameter range for '{paramDto.Name}': {ex.Message}", ex);
            }
        }

        return parameters;
    }
}


//public class JsonLoader : IConfigurationLoader
//{
//    private readonly JsonSerializerOptions _options = new()
//    {
//        PropertyNameCaseInsensitive = true,
//        WriteIndented = true,
//        Converters = { new JsonStringEnumConverter() }
//    };

//    public List<DeviceProfile> Load(string configFilePath)
//    {
//        if (!File.Exists(configFilePath))
//            throw new FileNotFoundException("Configuration file not found", configFilePath);

//        var json = File.ReadAllText(configFilePath);

//        List<DeviceProfile>? profile;

//        try
//        {
//            profile = JsonSerializer.Deserialize<List<DeviceProfile>>(json, _options);
//        }
//        catch (JsonException ex) 
//        {
//            throw new InvalidDataException("Invalid JSON format in configuration file", ex);
//        }
//        if (profile == null)
//            throw new InvalidDataException("Invalid or empty configuration");

//        return profile;
//    }

//}
