using TelemetryManager.Core.EventsArgs;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Services;


public class ParameterValidationService
{
    private readonly DeviceService _deviceService;

    public event EventHandler<ParameterOutOfRangeEventArgs>? ParameterOutOfRange;

    public ParameterValidationService(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task ValidateAsync(ushort deviceId, SensorId sensorId, string parameterName, double value)
    {
        var result = await _deviceService.CheckParameterValue(deviceId, sensorId, parameterName, value);

        if (!result.isValid)
        {
            var args = new ParameterOutOfRangeEventArgs(
                deviceId,
                sensorId,
                parameterName,
                value,
                result.currentInterval.Min,
                result.currentInterval.Max,
                result.deviation
            );

            OnParameterOutOfRange(args);
        }
    }

    protected virtual void OnParameterOutOfRange(ParameterOutOfRangeEventArgs e)
    {
        ParameterOutOfRange?.Invoke(this, e);
    }
}
