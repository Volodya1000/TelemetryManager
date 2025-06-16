namespace TelemetryManager.Core.Enums;

public enum SensorType : byte
{
    Temperature = 0x01,
    Pressure = 0x02,
    Accelerometer = 0x03,
    Magnetometer = 0x04,
    FreeFall = 0x5
}
