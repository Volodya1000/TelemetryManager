using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Utils;

namespace TelemetryManager.Core.TelemetryPackegesGenerator
{
    public class TelemetryGenerator
    {
        private readonly ushort _devId;
        private readonly int _totalPackets;
        private readonly byte[] _syncPattern = PacketConstants.SyncMarkerBytes;
        private Func<uint> _timeGenerator;
        private uint _currentTime;
        private double _noiseRatio;
        private readonly List<SensorConfig> _sensors = new List<SensorConfig>();
        private readonly Random _random = new Random();

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
            ms.Write(_syncPattern, 0, PacketConstants.SyncMarkerLength);             // Sync (4 bytes)
            WriteBigEndian(ms, _timeGenerator());       // Time (4 bytes)
            WriteBigEndian(ms, _devId);                 // DevId (2 bytes)
            ms.WriteByte(sensor.TypeId);                // TypeId (1 byte)
            ms.WriteByte(sensor.SourceId);              // SourceId (1 byte)
            WriteBigEndian(ms, (ushort)content.Length); // Size (2 bytes)
            ms.Write(content, 0, content.Length);       // Content
            if (needPadding) ms.WriteByte(0);           // Padding

            // Получаем данные БЕЗ синхромаркера для контрольной суммы
            var dataWithoutSync = ms.ToArray().Skip(4).ToArray();
            var checksum = ChecksumCalculator.Compute(dataWithoutSync);

            return ms.ToArray()
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

        private class SensorConfig
        {
            public byte TypeId { get; set; }
            public byte SourceId { get; set; }
            public Func<byte[]> ContentGenerator { get; set; }
        }
    }
}