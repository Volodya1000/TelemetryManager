using System.Buffers.Binary;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Utils;

namespace TelemetryManager.Infrastructure.Parsing
{
    public class PacketStreamParser : IPacketStreamParser
    {
        private const int MaxPacketSize = 1024;
        private readonly byte[] _syncMarker = PacketConstants.SyncMarkerBytes;
        private Stream _stream;

        public PacketParsingResult Parse(Stream stream)
        {
            _stream = stream;
            var packets = new List<TelemetryPacket>();
            var errors = new List<ParsingError>();

            while (true)
            {
                long packetStart = FindSyncMarker();
                if (packetStart < 0)
                    break;

                try
                {
                    var packet = ParseSinglePacket(packetStart);
                    packets.Add(packet);
                }
                catch (PacketParsingException ex)
                {
                    // Определяем границы «битого» участка
                    long errorEnd = FindNextSyncOrEnd();
                    var rawDataLength = errorEnd - packetStart;
                    byte[] rawData = ReadRaw(packetStart, rawDataLength);

                    errors.Add(new ParsingError(
                        StreamPosition: packetStart,
                        ErrorType: ex.ErrorType,
                        Message: ex.Message,
                        PacketStartOffset: packetStart,
                        PacketEndOffset: errorEnd,
                        RawData: rawData
                    ));

                    // Устанавливаем позицию на начало следующего синхромаркера
                    _stream.Position = errorEnd;
                }
            }

            return new PacketParsingResult(packets, errors);
        }

        private TelemetryPacket ParseSinglePacket(long packetStart)
        {
            // 1) читаем и парсим заголовок
            byte[] header = ReadExactly(PacketStructure.HeaderLength, ParsingErrorType.IncompletePacketHeader);
            var hdr = ParseHeader(header);

            // 2) проверяем размер
            ValidateSize(hdr.Size, hdr.Type);

            // 3) читаем payload
            byte[] payload = ReadExactly(hdr.Size, ParsingErrorType.ContentReadFailed);

            // 4) скипаем паддинг
            int padding = PacketStructure.CalculatePadding(hdr.Size);
            if (padding > 0)
                ReadExactly(padding, ParsingErrorType.PaddingReadFailed);

            // 5) проверяем контрольную сумму
            ValidateChecksum(header, payload, padding);

            // 6) парсим данные датчика
            var parser = SensorDataFactory.CreateParser(hdr.Type);
            parser.Parse(payload);

            return new TelemetryPacket(hdr.Time, hdr.DevId, hdr.Type, hdr.SourceId, payload, parser);
        }

        #region Helpers

        private long FindSyncMarker()
        {
            int b;
            while ((b = _stream.ReadByte()) != -1)
            {
                if (b != _syncMarker[0]) continue;
                long pos = _stream.Position - 1;
                // читаем потенциальный маркер
                Span<byte> buf = stackalloc byte[_syncMarker.Length];
                buf[0] = (byte)b;
                int read = _stream.Read(buf.Slice(1));
                if (read == _syncMarker.Length - 1 && buf.SequenceEqual(_syncMarker))
                    return pos;
                _stream.Position = pos + 1;
            }
            return -1;
        }

        private long FindNextSyncOrEnd()
        {
            long start = _stream.Position;
            if (FindSyncMarker() is long next && next >= 0)
                return next;
            return _stream.Length;
        }

        private byte[] ReadRaw(long offset, long length)
        {
            byte[] buffer = new byte[length];
            long old = _stream.Position;
            _stream.Position = offset;
            _stream.Read(buffer, 0, (int)length);
            _stream.Position = old;
            return buffer;
        }

        private byte[] ReadExactly(int count, ParsingErrorType errorType)
        {
            byte[] buf = new byte[count];
            int total = 0;
            while (total < count)
            {
                int r = _stream.Read(buf, total, count - total);
                if (r <= 0)
                    throw new PacketParsingException(errorType,
                        $"Failed to read {count} bytes (read {total}).");
                total += r;
            }
            return buf;
        }

        private (uint Time, ushort DevId, SensorType Type, byte SourceId, ushort Size) ParseHeader(byte[] data)
        {
            if (data.Length != PacketStructure.HeaderLength)
                throw new PacketParsingException(ParsingErrorType.InvalidPacketFormat,
                    "Header length mismatch.");

            uint time = BinaryPrimitives.ReadUInt32BigEndian(data);
            ushort devId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4));
            SensorType type = (SensorType)data[6];
            byte src = data[7];
            ushort size = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(8));

            return (time, devId, type, src, size);
        }

        private void ValidateSize(ushort size, SensorType type)
        {
            if (size == 0 || size > MaxPacketSize)
                throw new PacketParsingException(ParsingErrorType.InvalidPacketSize,
                    $"Unsupported packet size: {size}.");

            int expected = SensorDataFactory.GetExpectedLength(type);
            if (size != expected)
                throw new PacketParsingException(ParsingErrorType.InvalidPacketSize,
                    $"Size mismatch for {type}. Expected {expected}, got {size}.");
        }

        private void ValidateChecksum(byte[] header, byte[] payload, int padding)
        {
            int dataLen = header.Length + payload.Length + padding;
            byte[] data = PacketStructure.CombineArrays(header, payload, new byte[padding]);
            ushort expected = ReadUInt16();
            ushort actual = ChecksumCalculator.Compute(data);
            if (expected != actual)
                throw new PacketParsingException(ParsingErrorType.ChecksumMismatch,
                    $"Checksum fail: expected {expected}, actual {actual}.");
        }

        private ushort ReadUInt16()
        {
            Span<byte> buf = stackalloc byte[2];
            if (_stream.Read(buf) != 2)
                throw new PacketParsingException(ParsingErrorType.InvalidPacketFormat,
                    "Failed to read checksum bytes.");
            return BinaryPrimitives.ReadUInt16BigEndian(buf);
        }

        #endregion
    }

    public class PacketParsingException : Exception
    {
        public ParsingErrorType ErrorType { get; }
        public PacketParsingException(ParsingErrorType type, string message)
            : base(message) => ErrorType = type;
    }
}
