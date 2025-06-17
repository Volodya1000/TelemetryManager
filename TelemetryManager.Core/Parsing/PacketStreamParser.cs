
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Core.Utils;

namespace TelemetryManager.Core.Parsing
{
    public class PacketStreamParser
    {
        private const int MaxPacketSize = 1024;
        private readonly Stream _stream; 
        private readonly byte[] _syncMarkerBytes = PacketConstants.SyncMarkerBytes;
        
        public PacketStreamParser(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public List<TelemetryPacket> Parse()
        {
            var packets = new List<TelemetryPacket>();

            while (true)
            {
                long syncPosition = FindSyncMarker();
                if (syncPosition == -1)
                    break;

                _stream.Position = syncPosition + 4; // Skip sync marker

                Header header = ReadHeader();
                if (header == null)
                {
                    Console.WriteLine($"Invalid header after sync marker at position {syncPosition}");
                    continue;
                }

                byte[] content = ReadContent(header.Size);
                if (content == null)
                {
                    Console.WriteLine($"Failed to read content for packet at position {syncPosition}");
                    continue;
                }

                int paddingSize = (header.Size % 2 == 0) ? 0 : 1;
                if (paddingSize > 0 && _stream.ReadByte() == -1)
                {
                    Console.WriteLine($"Failed to read padding for packet at position {syncPosition}");
                    continue;
                }

                byte[] csBytes = new byte[2];
                if (_stream.Read(csBytes, 0, 2) != 2)
                {
                    Console.WriteLine($"Failed to read checksum for packet at position {syncPosition}");
                    continue;
                }

                // Читаем контрольную сумму в big-endian
                ushort expectedCs = (ushort)((csBytes[0] << 8) | csBytes[1]);

                // Формируем данные для проверки (без синхромаркера)
                byte[] dataForChecksum = GetDataForChecksum(header, content, paddingSize);
                ushort actualCs = ChecksumCalculator.Compute(dataForChecksum);

                if (expectedCs != actualCs)
                {
                    Console.WriteLine($"Checksum mismatch for packet at position {syncPosition}. Expected: {expectedCs}, Actual: {actualCs}");
                    continue;
                }

                ISensorData sensorData;
                try
                {
                    sensorData = SensorDataFactory.CreateParser(header.TypeId);
                    sensorData.Parse(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating or parsing sensor data: {ex.Message}");
                    continue;
                }

                var packet = new TelemetryPacket(
                    header.Time,
                    header.DevId,
                    header.TypeId,
                    header.SourceId,
                    content,
                    sensorData
                );

                packets.Add(packet);
            }

            return packets;
        }

        private long FindSyncMarker()
        {
            int nextByte;
            while ((nextByte = _stream.ReadByte()) != -1)
            {
                if (nextByte == _syncMarkerBytes[0])
                {
                    long startPos = _stream.Position - 1;
                    byte[] buffer = new byte[PacketConstants.SyncMarkerLength];
                    buffer[0] = (byte)nextByte;

                    int bytesRead = _stream.Read(buffer, 1, PacketConstants.SyncMarkerLength - 1);
                    if (bytesRead < PacketConstants.SyncMarkerLength - 1)
                    {
                        _stream.Position = startPos + 1;
                        continue;
                    }

                    if (buffer.SequenceEqual(_syncMarkerBytes))
                        return startPos;
                    else
                        _stream.Position = startPos + 1;
                }
            }
            return -1;
        }

        private Header ReadHeader()
        {
            byte[] headerBuffer = new byte[10];
            if (_stream.Read(headerBuffer, 0, 10) != 10)
                return null;

            // Ручной парсинг big-endian
            uint time = (uint)(
                (headerBuffer[0] << 24) |
                (headerBuffer[1] << 16) |
                (headerBuffer[2] << 8) |
                headerBuffer[3]
            );

            ushort devId = (ushort)(
                (headerBuffer[4] << 8) |
                headerBuffer[5]
            );

            SensorType type;
            try
            {
                type = (SensorType)headerBuffer[6];
            }
            catch
            {
                return null;
            }

            byte sourceId = headerBuffer[7];

            ushort size = (ushort)(
                (headerBuffer[8] << 8) |
                headerBuffer[9]
            );

            if (size > MaxPacketSize)
                return null;

            return new Header(
                rawBytes: headerBuffer,
                time: time,
                devId: devId,
                typeId: type,
                sourceId: sourceId,
                size: size
            );
        }

        private byte[] ReadContent(int size)
        {
            byte[] content = new byte[size];
            int totalRead = 0;

            while (totalRead < size)
            {
                int bytesRead = _stream.Read(content, totalRead, size - totalRead);
                if (bytesRead == 0)
                    return null;

                totalRead += bytesRead;
            }

            return content;
        }

        private byte[] GetDataForChecksum(Header header, byte[] content, int paddingSize)
        {
            byte[] padding = new byte[paddingSize];
            byte[] result = new byte[header.RawBytes.Length + content.Length + padding.Length];

            Buffer.BlockCopy(header.RawBytes, 0, result, 0, header.RawBytes.Length);
            Buffer.BlockCopy(content, 0, result, header.RawBytes.Length, content.Length);
            Buffer.BlockCopy(padding, 0, result, header.RawBytes.Length + content.Length, padding.Length);

            return result;
        }
       

        private class Header
        {
            public byte[] RawBytes { get; }
            public uint Time { get; }
            public ushort DevId { get; }
            public SensorType TypeId { get; }
            public byte SourceId { get; }
            public ushort Size { get; }

            public Header(byte[] rawBytes, uint time, ushort devId, SensorType typeId, byte sourceId, ushort size)
            {
                RawBytes = rawBytes;
                Time = time;
                DevId = devId;
                TypeId = typeId;
                SourceId = sourceId;
                Size = size;
            }
        }
    }
}