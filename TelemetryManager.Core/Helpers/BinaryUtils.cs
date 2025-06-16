namespace TelemetryManager.Core.Helpers;

public static class BinaryUtils
{
    public static byte[] BigEndian(byte[] data, int start, int length)
    {
        var segment = new byte[length];
        Array.Copy(data, start, segment, 0, length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(segment);

        return segment;
    }
}
