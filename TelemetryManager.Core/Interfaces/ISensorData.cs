namespace TelemetryManager.Core.Interfaces;

public interface ISensorData
{
    void Parse(byte[] data);

    IReadOnlyDictionary<string, double> GetValues();
}
