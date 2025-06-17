using TelemetryManager.Core.Utils;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorData;

public class PressureData : ISensorData
{
    public float Value { get; private set; } // в hPa (гектопаскалях)

    public void Parse(byte[] data)
    {
        Value = BitConverter.ToSingle(BinaryUtils.BigEndian(data, 0, 4), 0);
    }

    public IReadOnlyDictionary<string, double> GetValues() => new Dictionary<string, double>
    {
        ["Pressure"] = Value
    };
}