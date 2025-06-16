using TelemetryManager.Core.Helpers;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorData;

public class TemperatureData : ISensorData
{
    public float Value { get; private set; }

    public void Parse(byte[] data)
    {
        Value = BitConverter.ToSingle(BinaryUtils.BigEndian(data, 0, 4), 0);
    }

    public IReadOnlyDictionary<string, double> GetValues() => new Dictionary<string, double>
    {
        ["Temperature"] = Value
    };
}