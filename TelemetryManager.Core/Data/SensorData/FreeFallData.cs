using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorData;

public class FreeFallData : ISensorData
{
    public bool IsFalling { get; private set; }

    public void Parse(byte[] data)
    {
        IsFalling = data[0] != 0;
    }

    public IReadOnlyDictionary<string, double> GetValues() => new Dictionary<string, double>
    {
        ["IsFalling"] = IsFalling ? 1 : 0
    };
}