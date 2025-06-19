using System.Collections.ObjectModel;
using TelemetryManager.Application.Mapping;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class DeviceService
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceService(IDeviceRepository deviceRepository)
    {
        _deviceRepository= deviceRepository;
    }

    public async Task AddAsync(ushort deviceId, string name)
    {
        var deviceName = new Name(name);
        var device = new DeviceProfile( deviceId,deviceName);
        await _deviceRepository.AddAsync(device);
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
      params (string name, string unit, double min, double max)[] parameters)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        var sensorId = new SensorId(sensorTypeId, sensorSourceId);
        var sensorNameVO = new Name(sensorName);

        var sensorParameterList = new Collection<SensorParameterProfile>();
        foreach (var (name,unit, min, max) in parameters)
        {
            var paramName = new ParameterName(name);
            var parameter = new SensorParameterProfile(paramName, unit, min, max);
            sensorParameterList.Add(parameter);
        }

        var sensor = new SensorProfile(sensorId, sensorNameVO, sensorParameterList);

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
        device.SetParameterInterval(sensorId, name, min, max);
        await _deviceRepository.UpdateAsync(device);
    }

    public async Task<DeviceProfileDto> GetDeviceDataAsync(ushort deviceId)
    {
         var device = await _deviceRepository.GetByIdAsync(deviceId);

        return device.ToDto();
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

    public async ValueTask<(bool isValid, double deviation, Interval currentInterval)> 
        CheckParameterValue(ushort deviceId,
                            SensorId sensorId,
                            string parameterName,
                            double parameterValue)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);

        var parameterNameVO = new ParameterName(parameterName);
        return device.CheckParameterValue(sensorId, parameterNameVO, parameterValue);
    }
}
