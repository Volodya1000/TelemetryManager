using System.Text.Json;
using System.Text.Json.Serialization;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Infrastructure.JsonConfigurationLoader.Dtos;

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
        return devicesDto.Select(MapDevice).ToList();
    }

    private DeviceProfile MapDevice(DeviceDto deviceDto)
    {
        // Проверка DeviceId
        if (deviceDto.DeviceId < ushort.MinValue || deviceDto.DeviceId > ushort.MaxValue)
        {
            throw new InvalidDataException(
                $"Device ID {deviceDto.DeviceId} is out of range for ushort (0-65535)");
        }

        var deviceName = new Name(deviceDto.Name);
        var device = new DeviceProfile(
            deviceId: (ushort)deviceDto.DeviceId,
            name: deviceName
        );

        foreach (var sensorDto in deviceDto.Sensors)
        {
            // Проверка SourceId
            if (sensorDto.SourceId < byte.MinValue || sensorDto.SourceId > byte.MaxValue)
            {
                throw new InvalidDataException(
                    $"Source ID {sensorDto.SourceId} is out of range for byte (0-255)");
            }

            var sensorId = new SensorId(sensorDto.TypeId, (byte)sensorDto.SourceId);
            var parameters = MapParameters(sensorDto.Parameters);

            var sensorName = new Name(sensorDto.Name);
            var sensorProfile = new SensorProfile(
                id: sensorId,
                name: sensorName,
                parameters: parameters
            );

            device.AddSensor(sensorProfile);
        }

        return device;
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
                var parameterName = new ParameterName(paramDto.Name);
                parameters.Add(new SensorParameterProfile(
                    name: parameterName,
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
