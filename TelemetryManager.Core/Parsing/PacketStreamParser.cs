using System.Buffers.Binary;
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
                long syncPosition;
                try
                {
                    syncPosition = FindSyncMarker();
                    if (syncPosition == -1) break;
                }
                catch (Exception ex)
                {
                    throw new PacketParsingException("Failed to find sync marker", ex);
                }

                _stream.Position = syncPosition + PacketConstants.SyncMarkerLength; // Skip sync marker

                byte[] headerBytes = new byte[PacketStructure.HeaderLength];
                int headerBytesRead;
                try
                {
                    headerBytesRead = _stream.Read(headerBytes, 0, PacketStructure.HeaderLength);
                }
                catch (Exception ex)
                {
                    throw new PacketParsingException("Failed to read header bytes", ex);
                }

                if (headerBytesRead != PacketStructure.HeaderLength)
                {
                    throw new PacketParsingException($"Incomplete header read. Expected: {PacketStructure.HeaderLength}, Actual: {headerBytesRead}");
                }

                if (!PacketStructure.TryParseHeader(headerBytes,
                    out uint time,
                    out ushort devId,
                    out SensorType typeId,
                    out byte sourceId,
                    out ushort size))
                {
                    throw new PacketParsingException("Invalid header format");
                }

                if (size > MaxPacketSize)
                {
                    SkipInvalidPacket(size);
                    throw new PacketParsingException($"Content size too large: {size}");
                }

                try
                {
                    int expectedLength = SensorDataFactory.GetExpectedLength(typeId);
                    if (size != expectedLength)
                    {
                        SkipInvalidPacket(size);
                        throw new PacketParsingException($"Size mismatch for {typeId}. Expected: {expectedLength}, Actual: {size}");
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    SkipInvalidPacket(size);
                    throw new PacketParsingException($"Unknown sensor type: {typeId}", ex);
                }

                byte[] content;
                try
                {
                    content = ReadContent(size);
                    if (content == null)
                    {
                        throw new PacketParsingException("Failed to read content");
                    }
                }
                catch (Exception ex)
                {
                    throw new PacketParsingException("Failed to read packet content", ex);
                }

                int paddingSize = PacketStructure.CalculatePadding(size);
                if (paddingSize > 0)
                {
                    try
                    {
                        if (_stream.ReadByte() == -1)
                        {
                            throw new PacketParsingException("Failed to read padding");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new PacketParsingException("Failed to read padding bytes", ex);
                    }
                }

                byte[] csBytes = new byte[2];
                try
                {
                    if (_stream.Read(csBytes, 0, 2) != 2)
                    {
                        throw new PacketParsingException("Failed to read checksum");
                    }
                }
                catch (Exception ex)
                {
                    throw new PacketParsingException("Failed to read checksum bytes", ex);
                }

                ushort expectedCs = BinaryPrimitives.ReadUInt16BigEndian(csBytes);
                byte[] dataForChecksum = PacketStructure.CombineArrays(headerBytes, content, new byte[paddingSize]);
                ushort actualCs = ChecksumCalculator.Compute(dataForChecksum);

                if (expectedCs != actualCs)
                {
                    throw new PacketParsingException($"Checksum mismatch. Expected: {expectedCs}, Actual: {actualCs}");
                }

                //try
                //{
                    ISensorData sensorData = SensorDataFactory.CreateParser(typeId);
                    sensorData.Parse(content);

                    packets.Add(new TelemetryPacket(
                        time,
                        devId,
                        typeId,
                        sourceId,
                        content,
                        sensorData
                    ));
                //}
                //catch (Exception ex)
                //{
                    //throw new PacketParsingException($"Error creating or parsing sensor data: {ex.Message}", ex);
                //}
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

        private void SkipInvalidPacket(int contentSize)
        {
            int padding = contentSize % 2;
            int totalSkip = contentSize + padding + 2; // content + padding + checksum
            if (_stream.Position + totalSkip <= _stream.Length)
                _stream.Position += totalSkip;
        }

        private byte[] ReadContent(int size)
        {
            if (size <= 0 || size > MaxPacketSize)
                throw new InvalidOperationException($"Invalid size: {size}");

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
    }

    public class PacketParsingException : Exception
    {
        public PacketParsingException(string message) : base(message) { }
        public PacketParsingException(string message, Exception innerException) : base(message, innerException) { }
    }
}