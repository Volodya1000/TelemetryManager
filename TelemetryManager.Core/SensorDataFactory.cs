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
            SensorType.Pressure => new PressureData(), 
            SensorType.Accelerometer => new AccelerometerData(),
            SensorType.Magnetometer => new MagnetometerData(),
            SensorType.FreeFall => new FreeFallData(),
            _ => throw new ArgumentException($"Unsupported sensor type: {type}")
        };
    }

    public static int GetExpectedLength(SensorType type) => type switch
    {
        SensorType.Temperature => 4,
        SensorType.Pressure => 4,
        SensorType.Accelerometer => 12,  // 3 оси * 4 байта
        SensorType.Magnetometer => 12,    // 3 оси * 4 байта
        SensorType.FreeFall => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
