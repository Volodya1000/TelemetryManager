
namespace TelemetryManager.Core.EventArgs;

public class PacketErrorEventArgs
{
    private string reason;
    private byte[] rawData;
    private DateTime utcNow;

    public PacketErrorEventArgs(string reason, byte[] rawData, DateTime utcNow)
    {
        this.reason = reason;
        this.rawData = rawData;
        this.utcNow = utcNow;
    }
}
