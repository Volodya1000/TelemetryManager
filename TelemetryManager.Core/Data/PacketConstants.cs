namespace TelemetryManager.Core.Data;

public static class PacketConstants
{
    public const uint SyncMarkerUint = 0xFAA0055F;
    public static readonly byte[] SyncMarkerBytes = { 0xFA, 0xA0, 0x05, 0x5F };
    public const int SyncMarkerLength = 4;
}