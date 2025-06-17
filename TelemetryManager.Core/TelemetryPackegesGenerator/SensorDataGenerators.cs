namespace TelemetryManager.Core.TelemetryPackegesGenerator;

public static class SensorDataGenerators
{
    private static readonly Random _random = new Random();

    public static byte[] GenerateTemperatureData()
    {
        float temp = _random.Next(-50, 50) / 10.0f; // -50.0°C to +50.0°C
        byte[] bytes = BitConverter.GetBytes(temp);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes); 
        }
        return bytes;
    }
    public static byte[] GeneratePressureData()
    {
        // Нормальное атмосферное давление: 950-1050 hPa
        float pressure = 950f + 100f * (float)_random.NextDouble();
        byte[] bytes = BitConverter.GetBytes(pressure);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes); 
        }
        return bytes;
    }


    public static byte[] GenerateAccelerometerData()
    {
        float x = -20f + 40f * (float)_random.NextDouble();
        float y = -20f + 40f * (float)_random.NextDouble();
        float z = -20f + 40f * (float)_random.NextDouble();
        return BitConverter.GetBytes(x).Reverse()
            .Concat(BitConverter.GetBytes(y).Reverse())
            .Concat(BitConverter.GetBytes(z).Reverse())
            .ToArray();
    }

    public static byte[] GenerateMagnetometerData()
    {
        float x = -100f + 200f * (float)_random.NextDouble(); // -100 to +100 µT
        float y = -100f + 200f * (float)_random.NextDouble();
        float z = -100f + 200f * (float)_random.NextDouble();
        return BitConverter.GetBytes(x).Reverse()
            .Concat(BitConverter.GetBytes(y).Reverse())
            .Concat(BitConverter.GetBytes(z).Reverse())
            .ToArray();
    }

    public static byte[] GenerateFreeFallData()
    {
        byte status = (byte)(_random.NextDouble() > 0.5 ? 1 : 0); // 0 - норма, 1 - падение
        return new byte[] { status };
    }
}