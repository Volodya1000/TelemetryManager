namespace TelemetryManager.Core.Data;

public static class PacketConstants
{
    public const uint SyncMarkerUint = 0xFAA0055F;

    public static readonly byte[] SyncMarkerBytes =
        BitConverter.GetBytes(SyncMarkerUint).Reverse().ToArray();

    public static int SyncMarkerLength => SyncMarkerBytes.Length;
}