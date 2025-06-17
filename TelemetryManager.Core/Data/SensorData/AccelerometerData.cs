using TelemetryManager.Core.Utils;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorData;

public class AccelerometerData : ISensorData
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    public void Parse(byte[] data)
    {
        X = BitConverter.ToSingle(BinaryUtils.BigEndian(data, 0, 4), 0);
        Y = BitConverter.ToSingle(BinaryUtils.BigEndian(data, 4, 4), 0);
        Z = BitConverter.ToSingle(BinaryUtils.BigEndian(data, 8, 4), 0);
    }

    public IReadOnlyDictionary<string, double> GetValues() => new Dictionary<string, double>
    {
        ["X"] = X,
        ["Y"] = Y,
        ["Z"] = Z
    };
}