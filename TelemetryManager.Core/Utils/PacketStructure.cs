using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.Utils;

public static class PacketStructure
{
    public const int HeaderLength = 10; // 4(time) + 2(devId) + 1(type) + 1(source) + 2(size)

    public static byte[] BuildHeaderBytes(uint time, ushort devId, SensorType typeId, byte sourceId, ushort size)
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

    public static bool TryParseHeader(byte[] data, out uint time, out ushort devId, out SensorType type, out byte sourceId, out ushort size)
    {
        if (data.Length < HeaderLength)
        {
            time = 0; devId = 0; type = 0; sourceId = 0; size = 0;
            return false;
        }

        time = (uint)(
            (data[0] << 24) |
            (data[1] << 16) |
            (data[2] << 8) |
            data[3]
        );

        devId = (ushort)(
            (data[4] << 8) |
            data[5]
        );

        type = (SensorType)data[6];
        sourceId = data[7];

        size = (ushort)(
            (data[8] << 8) |
            data[9]
        );

        return true;
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
