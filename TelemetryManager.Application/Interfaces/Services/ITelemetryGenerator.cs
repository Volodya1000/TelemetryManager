namespace TelemetryManager.Application.Interfaces.Services;

public interface ITelemetryGenerator
{
    public Task<byte[]> GeneratePackets(int packetsCount, double noiseRatio = 0, double validityRatio = 1.0);
}
