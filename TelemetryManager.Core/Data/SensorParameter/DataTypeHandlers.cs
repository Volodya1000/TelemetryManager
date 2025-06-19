using System.Buffers.Binary;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorParameter;


public class BoolHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(byte);

    public object ParseValue(ReadOnlySpan<byte> data)
        => data[0] != 0;

    public double ConvertToDouble(object value)
        => (bool)value ? 1.0 : 0.0;
}

public class SByteHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(sbyte);

    public object ParseValue(ReadOnlySpan<byte> data)
        => (sbyte)data[0];

    public double ConvertToDouble(object value)
        => (sbyte)value;
}

public class ByteHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(byte);

    public object ParseValue(ReadOnlySpan<byte> data)
        => data[0];

    public double ConvertToDouble(object value)
        => (byte)value;
}

public class ShortHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(short);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadInt16BigEndian(data);

    public double ConvertToDouble(object value)
        => (short)value;
}

public class UShortHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(ushort);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadUInt16BigEndian(data);

    public double ConvertToDouble(object value)
        => (ushort)value;
}

public class IntHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(int);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadInt32BigEndian(data);

    public double ConvertToDouble(object value)
        => (int)value;
}

public class UIntHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(uint);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadUInt32BigEndian(data);

    public double ConvertToDouble(object value)
        => (uint)value;
}

public class LongHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(long);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadInt64BigEndian(data);

    public double ConvertToDouble(object value)
        => (long)value;
}

public class ULongHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(ulong);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BinaryPrimitives.ReadUInt64BigEndian(data);

    public double ConvertToDouble(object value)
        => (ulong)value;
}

public class FloatHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(float);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(data));

    public double ConvertToDouble(object value)
        => (float)value;
}

public class DoubleHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(double);

    public object ParseValue(ReadOnlySpan<byte> data)
        => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(data));

    public double ConvertToDouble(object value)
        => (double)value;
}

public class DecimalHandler : IDataTypeHandler
{
    public int GetSize() => sizeof(decimal);

    public object ParseValue(ReadOnlySpan<byte> data)
    {
        // Разбиваем 16 байт на 4 части по 4 байта
        int[] parts = new int[4];
        for (int i = 0; i < 4; i++)
        {
            parts[i] = BinaryPrimitives.ReadInt32BigEndian(data.Slice(i * 4, 4));
        }

        return new decimal(parts);
    }

    public double ConvertToDouble(object value)
        => decimal.ToDouble((decimal)value);
}
