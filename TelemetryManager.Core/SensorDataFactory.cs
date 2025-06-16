using TelemetryManager.Core.Data.SensorData;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core;

public static class SensorDataFactory
{
    public static ISensorData CreateParser(SensorType type)
    {
        return type switch
        {
            SensorType.Temperature => new TemperatureData(),
            SensorType.Accelerometer => new AccelerometerData(),
            _ => throw new ArgumentException($"Unsupported sensor type: {type}")
        };
    }
}
