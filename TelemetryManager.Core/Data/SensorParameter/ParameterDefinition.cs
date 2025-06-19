using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace TelemetryManager.Core.Data.SensorParameter;

public class ParameterDefinition
{
    public string Name { get; }
    public string Quantity { get; }
    public string Unit { get; }
    public Type DataType { get; }

    public ParameterDefinition(string name, string quantity, string unit, Type dataType)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
        DataType = dataType;
    }

    public int ByteSize => DataType switch
    {
        var t when t == typeof(bool) => sizeof(byte),
        var t when t == typeof(sbyte) => sizeof(sbyte),
        var t when t == typeof(byte) => sizeof(byte),
        var t when t == typeof(short) => sizeof(short),
        var t when t == typeof(ushort) => sizeof(ushort),
        var t when t == typeof(int) => sizeof(int),
        var t when t == typeof(uint) => sizeof(uint),
        var t when t == typeof(long) => sizeof(long),
        var t when t == typeof(ulong) => sizeof(ulong),
        var t when t == typeof(float) => sizeof(float),
        var t when t == typeof(double) => sizeof(double),
        var t when t == typeof(decimal) => sizeof(decimal),
        _ => throw new NotSupportedException($"Тип {DataType.Name} не поддерживается")
    };
}

public class ContentDefinition
{
    public byte TypeId { get; }
    public string Name { get; }
    public IReadOnlyList<ParameterDefinition> Parameters { get; }
    public int TotalSizeBytes { get; }

    public ContentDefinition(byte typeId, string name, IEnumerable<ParameterDefinition> parameters)
    {
        TypeId = typeId;
        Name = name;
        Parameters = parameters.ToList().AsReadOnly();
        TotalSizeBytes = Parameters.Sum(p => p.ByteSize);
    }
}

public class SensorContentRegistry
{
    private readonly ConcurrentDictionary<byte, ContentDefinition> _defs = new();

    public void Register(ContentDefinition definition)
    {
        if (!_defs.TryAdd(definition.TypeId, definition))
            throw new InvalidOperationException($"TypeId {definition.TypeId} уже зарегистрирован");
    }

    public bool IsRegistered(byte typeId) => _defs.ContainsKey(typeId);

    public ContentDefinition GetDefinition(byte typeId) =>
        _defs.TryGetValue(typeId, out var def) ? def : throw new KeyNotFoundException($"TypeId {typeId} не найден");

    public string GetName(byte typeId) => GetDefinition(typeId).Name;

    public IEnumerable<ContentDefinition> AllDefinitions => _defs.Values;
}

public class SensorParser
{
    private readonly SensorContentRegistry _registry;
    public SensorParser(SensorContentRegistry registry) => _registry = registry;

    public IReadOnlyDictionary<string, double> Parse(byte typeId, byte[] data)
    {
        var def = _registry.GetDefinition(typeId);
        if (data.Length < def.TotalSizeBytes)
            throw new ArgumentException($"Нужно {def.TotalSizeBytes} байт, получили {data.Length}");

        var res = new Dictionary<string, double>(def.Parameters.Count);
        int offset = 0;
        foreach (var p in def.Parameters)
        {
            var span = new ReadOnlySpan<byte>(data, offset, p.ByteSize);
            offset += p.ByteSize;
            double val = p.DataType switch
            {
                var t when t == typeof(bool) => span[0] == 1 ? 1.0 : 0.0,
                var t when t == typeof(sbyte) => (sbyte)span[0],
                var t when t == typeof(byte) => span[0],
                var t when t == typeof(short) => BinaryPrimitives.ReadInt16BigEndian(span),
                var t when t == typeof(ushort) => BinaryPrimitives.ReadUInt16BigEndian(span),
                var t when t == typeof(int) => BinaryPrimitives.ReadInt32BigEndian(span),
                var t when t == typeof(uint) => BinaryPrimitives.ReadUInt32BigEndian(span),
                var t when t == typeof(long) => BinaryPrimitives.ReadInt64BigEndian(span),
                var t when t == typeof(ulong) => BinaryPrimitives.ReadUInt64BigEndian(span),
                var t when t == typeof(float) => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(span)),
                var t when t == typeof(double) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span)),
                _ => throw new InvalidOperationException($"Невозможно распарсить {p.DataType.Name}")
            };
            res[p.Name] = val;
        }
        return res;
    }
}