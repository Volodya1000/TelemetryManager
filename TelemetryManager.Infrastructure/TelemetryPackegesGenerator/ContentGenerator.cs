using System.Buffers.Binary;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

public class ContentGenerator
{
    private readonly IContentDefinitionRepository _repository;
    private readonly Random _random = new();

    public ContentGenerator(IContentDefinitionRepository repository) => _repository = repository;

    public async Task<byte[]> GenerateContentAsync(SensorProfile sensorProfile, bool shouldBeValid)
    {
        var contentDef = await _repository.GetDefinitionAsync(sensorProfile.TypeId);
        var content = new byte[contentDef.TotalSizeBytes];
        int offset = 0;

        foreach (var paramDef in contentDef.Parameters)
        {
            var sensorParam = sensorProfile.GetParameter(paramDef.Name);
            var value = GenerateValue(paramDef, sensorParam.CurrentInterval, shouldBeValid);
            offset = WriteValue(content, offset, value, paramDef.DataType, paramDef.ByteSize);
        }

        return content;
    }

    private object GenerateValue(ParameterDefinition paramDef, Interval interval, bool shouldBeValid)
    {
        var (minPossible, maxPossible) = GetTypeRange(paramDef.DataType);
        double minInterval = interval.Min;
        double maxInterval = interval.Max;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            double doubleValue;

            if (shouldBeValid)
            {
                // Генерация валидного значения
                doubleValue = minInterval + _random.NextDouble() * (maxInterval - minInterval);
            }
            else
            {
                // Генерация гарантированно невалидного значения
                doubleValue = GenerateInvalidValue(minInterval, maxInterval, minPossible, maxPossible);
            }

            var value = ConvertDouble(doubleValue, paramDef.DataType);
            double convertedValue = paramDef.Handler.ConvertToDouble(value);
            bool isValid = interval.Contains(convertedValue);

            if (shouldBeValid == isValid)
                return value;
        }

        // Если после 10 попыток не удалось — возвращаем хоть что-то приближённое
        return ConvertDouble(
            minInterval + _random.NextDouble() * (maxInterval - minInterval),
            paramDef.DataType
        );
    }

    private double GenerateInvalidValue(double minValid, double maxValid, double absMin, double absMax)
    {
        // Случайный коэффициент от 0.1 до 3.0
        double multiplier = 0.1 + _random.NextDouble() * (3.0 - 0.1);

        bool canGoBelow = minValid > absMin;
        bool canGoAbove = maxValid < absMax;

        if (canGoBelow && canGoAbove)
        {
            if (_random.Next(2) == 0)
                return ShiftBelow(minValid, multiplier, absMin);
            else
                return ShiftAbove(maxValid, multiplier, absMax);
        }
        else if (canGoBelow)
        {
            return ShiftBelow(minValid, multiplier, absMin);
        }
        else if (canGoAbove)
        {
            return ShiftAbove(maxValid, multiplier, absMax);
        }
        else
        {
            // Весь диапазон допустим — не можем выйти за границы
            return minValid + _random.NextDouble() * (maxValid - minValid);
        }
    }

    private double ShiftBelow(double minValid, double multiplier, double absMin)
    {
        double shift = Math.Max(1.0, Math.Abs(minValid) * multiplier);
        double result = minValid - shift;
        return result < absMin ? absMin : result;
    }

    private double ShiftAbove(double maxValid, double multiplier, double absMax)
    {
        double shift = Math.Max(1.0, Math.Abs(maxValid) * multiplier);
        double result = maxValid + shift;
        return result > absMax ? absMax : result;
    }

    private int WriteValue(byte[] buffer, int offset, object value, Type type, int byteSize)
    {
        var span = new Span<byte>(buffer, offset, byteSize);

        switch (value)
        {
            case bool b: span[0] = b ? (byte)1 : (byte)0; break;
            case sbyte sb: span[0] = (byte)sb; break;
            case byte b: span[0] = b; break;
            case short s: BinaryPrimitives.WriteInt16BigEndian(span, s); break;
            case ushort us: BinaryPrimitives.WriteUInt16BigEndian(span, us); break;
            case int i: BinaryPrimitives.WriteInt32BigEndian(span, i); break;
            case uint ui: BinaryPrimitives.WriteUInt32BigEndian(span, ui); break;
            case long l: BinaryPrimitives.WriteInt64BigEndian(span, l); break;
            case ulong ul: BinaryPrimitives.WriteUInt64BigEndian(span, ul); break;
            case float f:
                BinaryPrimitives.WriteInt32BigEndian(span, BitConverter.SingleToInt32Bits(f));
                break;
            case double d:
                BinaryPrimitives.WriteInt64BigEndian(span, BitConverter.DoubleToInt64Bits(d));
                break;
            case decimal dec:
                int[] bits = decimal.GetBits(dec);
                for (int i = 0; i < 4; i++)
                    BinaryPrimitives.WriteInt32BigEndian(span.Slice(i * 4, 4), bits[i]);
                break;
            default: throw new NotSupportedException($"Unsupported type: {type}");
        }

        return offset + byteSize;
    }

    private object ConvertDouble(double value, Type type) => type switch
    {
        _ when type == typeof(bool) => value >= 0.5,
        _ when type == typeof(sbyte) => (sbyte)Math.Round(value),
        _ when type == typeof(byte) => (byte)Math.Round(value),
        _ when type == typeof(short) => (short)Math.Round(value),
        _ when type == typeof(ushort) => (ushort)Math.Round(value),
        _ when type == typeof(int) => (int)Math.Round(value),
        _ when type == typeof(uint) => (uint)Math.Round(value),
        _ when type == typeof(long) => (long)Math.Round(value),
        _ when type == typeof(ulong) => (ulong)Math.Round(value),
        _ when type == typeof(float) => (float)value,
        _ when type == typeof(double) => value,
        _ when type == typeof(decimal) => (decimal)value,
        _ => throw new NotSupportedException($"Unsupported type: {type}")
    };

    private (double Min, double Max) GetTypeRange(Type type) => type switch
    {
        _ when type == typeof(bool) => (0, 1),
        _ when type == typeof(sbyte) => (sbyte.MinValue, sbyte.MaxValue),
        _ when type == typeof(byte) => (byte.MinValue, byte.MaxValue),
        _ when type == typeof(short) => (short.MinValue, short.MaxValue),
        _ when type == typeof(ushort) => (ushort.MinValue, ushort.MaxValue),
        _ when type == typeof(int) => (int.MinValue, int.MaxValue),
        _ when type == typeof(uint) => (uint.MinValue, uint.MaxValue),
        _ when type == typeof(long) => (long.MinValue, long.MaxValue),
        _ when type == typeof(ulong) => (ulong.MinValue, ulong.MaxValue),
        _ when type == typeof(float) => (float.MinValue, float.MaxValue),
        _ when type == typeof(double) => (double.MinValue, double.MaxValue),
        _ when type == typeof(decimal) => ((double)decimal.MinValue, (double)decimal.MaxValue),
        _ => throw new NotSupportedException($"Unsupported type: {type}")
    };
}
