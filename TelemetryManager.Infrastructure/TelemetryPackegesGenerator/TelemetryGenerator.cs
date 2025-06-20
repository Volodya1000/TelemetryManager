using System.Buffers.Binary;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Core.Utils;
using TelemetryManager.Infrastructure.Parsing.Data;

namespace TelemetryManager.Infrastructure.TelemetryPackegesGenerator
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
        IContentDefinitionRepository _сontentDefinitionRepository;

        public TelemetryGenerator(IContentDefinitionRepository contentDefinitionRepository,ushort devId, int totalPackets, double noiseRatio)
        {
            if (totalPackets < 1)
                throw new ArgumentException("Total packets must be at least 1", nameof(totalPackets));

            if (noiseRatio < 0 || noiseRatio > 1)
                throw new ArgumentException("Noise ratio must be between 0.0 and 1.0");
            _devId = devId;
            _totalPackets = totalPackets;
            _timeGenerator = DefaultTimeGenerator;
            _noiseRatio = noiseRatio;
            _сontentDefinitionRepository= contentDefinitionRepository;
        }

        private uint DefaultTimeGenerator()
        {
            _currentTime += (uint)_random.Next(1, 101);
            return _currentTime;
        }

        public TelemetryGenerator SetTimeGenerator(Func<uint> timeGenerator)
        {
            _timeGenerator = timeGenerator ?? DefaultTimeGenerator;
            return this;
        }

        public TelemetryGenerator AddSensor(byte type, byte sourceId, Func<byte[]> contentGenerator)
        {
            // Валидация длины при добавлении сенсора
            int expectedLength = _сontentDefinitionRepository.GetDefinitionAsync(type).Result.TotalSizeBytes;//Исправить

            _sensors.Add(new SensorConfig
            {
                TypeId = (byte)type,
                SourceId = sourceId,
                ContentGenerator = () =>
                {
                    byte[] data = contentGenerator();
                    if (data.Length != expectedLength)
                    {
                        throw new InvalidOperationException(
                            $"Content length mismatch for {type}. Expected: {expectedLength}, Actual: {data.Length}");
                    }
                    return data;
                }
            });

            return this;
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
            byte[] content = sensor.ContentGenerator();

            uint time = _timeGenerator();
            int paddingSize = PacketHelper.CalculatePadding(content.Length);

            byte[] headerBytes = PacketHelper.BuildHeaderBytes(
                time,
                _devId,
                sensor.TypeId,
                sensor.SourceId,
                (ushort)content.Length
            );

            using var ms = new MemoryStream();
            ms.Write(_syncPattern, 0, PacketConstants.SyncMarkerLength);
            ms.Write(headerBytes, 0, headerBytes.Length);
            ms.Write(content, 0, content.Length);

            if (paddingSize > 0) ms.WriteByte(0);

            // Вычисление контрольной суммы (без синхромаркера)
            byte[] dataForChecksum = PacketHelper.CombineArrays(
                headerBytes,
                content,
                new byte[paddingSize]
            );

            ushort checksum = ChecksumCalculator.Compute(dataForChecksum);
            byte[] csBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(csBytes, checksum);
            ms.Write(csBytes, 0, 2);

            return ms.ToArray();
        }


        private void CorruptPacketIfNeeded(ref byte[] packet)
        {
            if (_random.NextDouble() >= _noiseRatio) return;

            // Повреждаем случайный байт в пакете
            var index = _random.Next(packet.Length);
            packet[index] = (byte)_random.Next(256);
        }

        private class SensorConfig
        {
            public byte TypeId { get; set; }
            public byte SourceId { get; set; }
            public Func<byte[]> ContentGenerator { get; set; }
        }
    }
}