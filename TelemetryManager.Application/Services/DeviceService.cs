using System.Collections.ObjectModel;
using System.Xml.Linq;
using TelemetryManager.Application.Mapping;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Data.ValueObjects.HistoryRecords;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class DeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IContentDefinitionRepository _contentDefinitionRepository;

    public DeviceService(IDeviceRepository deviceRepository, IContentDefinitionRepository contentDefinitionRepository)
    {
        _deviceRepository= deviceRepository;
        _contentDefinitionRepository = contentDefinitionRepository;
    }

    public async Task AddAsync(ushort deviceId, string name)
    {
        var deviceName = new Name(name);
        var device = new DeviceProfile( deviceId,deviceName);
        await _deviceRepository.AddAsync(device);
    }

     public async Task<IEnumerable<DeviceDto>> GetAllAsync()
     {
        return (await _deviceRepository.GetAllAsync()).Select(d=>d.ToDto());
     }


     

    internal async Task SetActivationTimeAsync(ushort deviceId, DateTime activationTime)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        device.SetActivationTime(activationTime);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task AddSensorWithParametersAsync(
       ushort deviceId,
       byte sensorTypeId,
       byte sensorSourceId,
       string sensorName,
       IReadOnlyDictionary<string, Interval> initialIntervals = null) 
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        var sensorNameVO = new Name(sensorName);

        var sensorParameterList = new Collection<SensorParameterProfile>();
        var contentDefinition = await _contentDefinitionRepository.GetDefinitionAsync(sensorTypeId)
            ?? throw new ArgumentException($"TypeId {sensorTypeId} does not exist", nameof(sensorTypeId));

        foreach (var parameterDefinition in contentDefinition.Parameters)
        {
            var interval = initialIntervals != null && initialIntervals.TryGetValue(parameterDefinition.Name.Value, out var initialInterval)
                ? initialInterval
                : new Interval(double.MinValue, double.MaxValue);

            var parameter = new SensorParameterProfile(parameterDefinition.Name, interval, DateTime.Now);
            sensorParameterList.Add(parameter);
        }

        var sensor = new SensorProfile(sensorId, sensorNameVO, sensorParameterList);
        sensor.MarkConnected(DateTime.Now);
        device.AddSensor(sensor);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task MarkSensorConnectedAsync(
        ushort deviceId, byte sensorTypeId, byte sensorSourceId, DateTime timestamp)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        device.MarkSensorConnected(sensorId, timestamp);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task MarkSensorDisconnectedAsync(
        ushort deviceId, byte sensorTypeId, byte sensorSourceId, DateTime timestamp)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        device.MarkSensorDisconnected(sensorId, timestamp);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task<bool> IsSensorConnectedAtAsync(
        ushort deviceId, byte sensorTypeId, byte sensorSourceId, DateTime timestamp)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        return device.IsSensorConnectedAt(sensorId, timestamp);
    }

    public async Task<bool> IsSensorCurrentlyConnectedAsync(
    ushort deviceId, byte sensorTypeId, byte sensorSourceId)
    {
        return await IsSensorConnectedAtAsync(
            deviceId,
            sensorTypeId,
            sensorSourceId,
            DateTime.Now); 
    }

    public async Task SetParameterIntervalAsync(
       ushort deviceId,
       byte sensorTypeId,
       byte sensorSourceId,
       string parameterName,
       double min,
       double max)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        var name = new ParameterName(parameterName);
        device.SetParameterInterval(sensorId, name, new Interval(min, max), DateTime.Now);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task<DeviceProfile> GetDeviceDataAsync(ushort deviceId)
    {
         var device = await _deviceRepository.GetByIdAsync(deviceId);

        return device;//.ToDto();
    }

    public async Task<Dictionary<ushort, IReadOnlyCollection<SensorId>>> GetAllDeviceSensorsIdsDictionaryAsync()
    {
        var devices = await _deviceRepository.GetAllAsync();
        return devices.ToDictionary(d => d.DeviceId, d => d.SensorIds);
    }

    public async Task<IEnumerable<ushort>> GetDevicesWithoutActivationTimeAsync()
    {
        var devices = await _deviceRepository.GetAllAsync();
        return devices
            .Where(d => !d.ActivationTime.HasValue)
            .Select(d => d.DeviceId);
    }

    public async Task<(bool isValid, double deviation, Interval currentInterval)>
       CheckParameterValue(ushort deviceId,
                          byte typeId,
                          byte sourceId,
                          string parameterName,
                          double parameterValue,
                          DateTime dateTime)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var parameterNameVO = new ParameterName(parameterName);

        var result = device.CheckParameterValue(
            new SensorId(typeId, sourceId),
            parameterNameVO,
            parameterValue,
            dateTime);

        return result;
    }

    public async Task<ParameterIntervalDto> GetParameterInterval(
      ushort deviceId,
      byte typeId,
      byte sourceId,
      string parameterName,
      DateTime dateTime)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException("Parameter name cannot be empty or whitespace", nameof(parameterName));

        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new KeyNotFoundException($"Device with ID {deviceId} not found");

        var sensor = device.Sensors
            .FirstOrDefault(s => s.Id == new SensorId(typeId, sourceId));
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor with TypeID {typeId} and SourceID {sourceId} not found in device {deviceId}");

        var parameter = sensor.Parameters
            .FirstOrDefault(p => p.Name.Value == parameterName);
        if (parameter == null)
            throw new KeyNotFoundException($"Parameter '{parameterName}' not found in sensor {typeId}/{sourceId}");

        var currentInterval= parameter.GetIntervalForDate(dateTime);

        var definition = await _contentDefinitionRepository.GetDefinitionAsync(typeId);
        if (definition == null)
            throw new KeyNotFoundException($"Content definition not found for TypeID {typeId}");

        var currentParameter = definition.Parameters
            .FirstOrDefault(p => p.Name.Value == parameterName);
        if (currentParameter == null)
            throw new KeyNotFoundException($"Parameter definition '{parameterName}' not found in content definition for TypeID {typeId}");

        return new ParameterIntervalDto(
            currentInterval.Min,
            currentInterval.Max,
            currentParameter.Unit,
            currentParameter.Quantity
        );
    }

    public async Task SetParameterIntervalAsync(
    ushort deviceId,
    SensorId sensorId,
    string parameterName,
    double min,
    double max)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var name = new ParameterName(parameterName);
        device.SetParameterInterval(sensorId, name, new Interval(min, max), DateTime.Now);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task<IEnumerable<DeviceFilterDto>> GetDevicesForFilterAsync()
    {
        var devices = await _deviceRepository.GetAllAsync();

        var result = new List<DeviceFilterDto>();

        foreach (var device in devices)
        {
            var sensorsDto = new List<SensorFilterDto>();

            foreach (var sensor in device.Sensors)
            {
                var contentDefinition = await _contentDefinitionRepository.GetDefinitionAsync(sensor.Id.TypeId);
                var contentTypeDto = contentDefinition?.ToFilterDto();

                var sensorDto = sensor.ToFilterDto(contentTypeDto);
                sensorsDto.Add(sensorDto);
            }

            var deviceDto = device.ToFilterDto(sensorsDto);
            result.Add(deviceDto);
        }

        return result;
    }

    public async Task<IEnumerable<ParameterIntervalChangeRecord>> GetParameterHistory(ushort deviceId,
                                          byte typeId,
                                          byte sourceId,
                                          string parameterName)
    {

        var device = await _deviceRepository.GetByIdAsync(deviceId);

        if (device == null)
            throw new KeyNotFoundException($"Device with ID {deviceId} not found");

        var sensor = device.Sensors
            .FirstOrDefault(s => s.Id == new SensorId(typeId, sourceId));
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor with TypeID {typeId} and SourceID {sourceId} not found in device {deviceId}");

        var parameter = sensor.Parameters
            .FirstOrDefault(p => p.Name.Value == parameterName);
        if (parameter == null)
            throw new KeyNotFoundException($"Parameter '{parameterName}' not found in sensor {typeId}/{sourceId}");

        return parameter.IntervalHistory;
    }

}
