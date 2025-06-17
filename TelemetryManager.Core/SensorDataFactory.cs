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

    public static int GetExpectedLength(SensorType type) => type switch
    {
        SensorType.Temperature => 4,
        SensorType.Pressure => 2,
        SensorType.Accelerometer => 6,  // 3 оси * 2 байта
        SensorType.Magnetometer => 6,   // 3 оси * 2 байта
        SensorType.FreeFall => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
