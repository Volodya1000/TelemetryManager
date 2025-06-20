namespace TelemetryManager.Core.Utils;

public static class PacketHelper
{
    public const int HeaderLength = 10; // 4(time) + 2(devId) + 1(type) + 1(source) + 2(size)

    public static byte[] BuildHeaderBytes(uint time, ushort devId, byte typeId, byte sourceId, ushort size)
    {
        byte[] headerBytes = new byte[HeaderLength];
        // Time (4 bytes big-endian)
        headerBytes[0] = (byte)(time >> 24);
        headerBytes[1] = (byte)(time >> 16);
        headerBytes[2] = (byte)(time >> 8);
        headerBytes[3] = (byte)time;
        // Device ID (2 bytes big-endian)
        headerBytes[4] = (byte)(devId >> 8);
        headerBytes[5] = (byte)devId;
        // Sensor type
        headerBytes[6] = (byte)typeId;
        // Source ID
        headerBytes[7] = sourceId;
        // Content size (2 bytes big-endian)
        headerBytes[8] = (byte)(size >> 8);
        headerBytes[9] = (byte)size;

        return headerBytes;
    }

    

    public static int CalculatePadding(int contentSize) => contentSize % 2 == 0 ? 0 : 1;

    public static byte[] CombineArrays(params byte[][] arrays)
    {
        int totalLength = arrays.Sum(a => a.Length);
        byte[] result = new byte[totalLength];
        int offset = 0;

        foreach (byte[] array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }
}
