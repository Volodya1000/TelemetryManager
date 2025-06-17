using TelemetryManager.Core.Enums;

namespace TelemetryManager.Core.TelemetryPackegesGenerator;

public class TelemetryGenerator
{
    private readonly ushort _devId;
    private readonly int _totalPackets;
    private Func<uint> _timeGenerator;
    private uint _currentTime;
    private double _noiseRatio;
    private readonly List<SensorConfig> _sensors = new List<SensorConfig>();
    private readonly Random _random = new Random();
    private readonly byte[] _syncPattern = { 0xFA, 0xA0, 0x05, 0x5F };

    public TelemetryGenerator(ushort devId, int totalPackets)
    {
        if (totalPackets < 1)
            throw new ArgumentException("Total packets must be at least 1", nameof(totalPackets));

        _devId = devId;
        _totalPackets = totalPackets;
        _timeGenerator = DefaultTimeGenerator;
    }

    private uint DefaultTimeGenerator()
    {
        _currentTime += (uint)_random.Next(1, 101);
        return _currentTime;
    }

    public TelemetryGenerator SetNoiseRatio(double ratio)
    {
        if (ratio < 0 || ratio > 1)
            throw new ArgumentException("Noise ratio must be between 0.0 and 1.0");
        _noiseRatio = ratio;
        return this;
    }

    public TelemetryGenerator SetTimeGenerator(Func<uint> timeGenerator)
    {
        _timeGenerator = timeGenerator ?? DefaultTimeGenerator;
        return this;
    }

    public TelemetryGenerator AddSensor(byte typeId, byte sourceId, Func<byte[]> contentGenerator)
    {
        if (contentGenerator == null)
            throw new ArgumentNullException(nameof(contentGenerator));

        _sensors.Add(new SensorConfig
        {
            TypeId = typeId,
            SourceId = sourceId,
            ContentGenerator = contentGenerator
        });
        return this;
    }

    public TelemetryGenerator AddSensor(SensorType type, byte sourceId, Func<byte[]> contentGenerator)
    {
        return AddSensor((byte)type, sourceId, contentGenerator);
    }

    public void Generate(string filePath)
    {
        if (_sensors.Count == 0)
            throw new InvalidOperationException("At least one sensor must be added");

        using var fileStream = new FileStream(filePath, FileMode.Create);
        for (int i = 0; i < _totalPackets; i++)
        {
            GenerateNoise(fileStream);
            var packet = GenerateValidPacket();
            CorruptPacketIfNeeded(ref packet);
            fileStream.Write(packet, 0, packet.Length);
        }
    }

    private void GenerateNoise(Stream stream)
    {
        if (_random.NextDouble() >= _noiseRatio) return;

        var noise = new byte[_random.Next(1, 6)];
        _random.NextBytes(noise);
        stream.Write(noise, 0, noise.Length);
    }

    private byte[] GenerateValidPacket()
    {
        var sensor = _sensors[_random.Next(_sensors.Count)];
        var content = sensor.ContentGenerator();
        bool needPadding = (content.Length % 2) != 0;

        using var ms = new MemoryStream();
        // Header
        ms.Write(_syncPattern, 0, 4);
        WriteBigEndian(ms, _timeGenerator());  // Time
        WriteBigEndian(ms, _devId);            // DevId
        ms.WriteByte(sensor.TypeId);           // TypeId
        ms.WriteByte(sensor.SourceId);         // SourceId
        WriteBigEndian(ms, (ushort)content.Length); // Size
        ms.Write(content, 0, content.Length);  // Content
        if (needPadding) ms.WriteByte(0);      // Padding

        // Calculate and add checksum
        var headerAndPayload = ms.ToArray();
        var checksum = CalculateChecksum(headerAndPayload);
        return headerAndPayload
            .Concat(new[] { (byte)(checksum >> 8), (byte)checksum })
            .ToArray();
    }

    private void CorruptPacketIfNeeded(ref byte[] packet)
    {
        if (_random.NextDouble() >= _noiseRatio) return;

        // Повреждаем случайный байт в пакете
        var index = _random.Next(packet.Length);
        packet[index] = (byte)_random.Next(256);
    }

    private void WriteBigEndian(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private void WriteBigEndian(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private ushort CalculateChecksum(byte[] data)
    {
        uint sum = 0;
        for (int i = 0; i < data.Length; i += 2)
        {
            ushort word = (i == data.Length - 1)
                ? (ushort)(data[i] << 8)
                : (ushort)((data[i] << 8) | data[i + 1]);
            sum += word;
        }
        while ((sum >> 16) != 0)
            sum = (sum & 0xFFFF) + (sum >> 16);
        return (ushort)~sum;
    }

    private class SensorConfig
    {
        public byte TypeId { get; set; }
        public byte SourceId { get; set; }
        public Func<byte[]> ContentGenerator { get; set; }
    }
}