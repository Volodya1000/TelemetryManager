using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Core.Utils;

namespace TelemetryManager.Infrastructure.Parsing;

//public enum ParsingErrorType
//{
//    SyncMarkerNotFound,
//    IncompleteHeader,
//    InvalidHeaderFormat,
//    SizeMismatch,
//    UnknownSensorType,
//    ContentReadFailed,
//    ChecksumMismatch,
//    PaddingReadFailed,
//    DataValidationFailed
//}

//public record ParsingError(
//    long StreamPosition,
//    ParsingErrorType ErrorType,
//    string Message,
//    long PacketStartOffset,
//    ushort? DeviceId = null,
//    SensorType? SensorType = null
//);


public class PacketStreamParser : IPacketStreamParser
{
    private const int MaxPacketSize = 1024;
    private Stream _stream;
    private readonly byte[] _syncMarkerBytes = PacketConstants.SyncMarkerBytes;

    public PacketParsingResult Parse(Stream stream)
    {
        _stream = stream;
        var packets = new List<TelemetryPacket>();
        var errors = new List<ParsingError>();

        while (true)
        {
            // Find sync marker
            long syncPos = FindSyncMarker();
            if (syncPos < 0)
                break;

            long packetStart = syncPos;
            _stream.Position = packetStart + PacketConstants.SyncMarkerLength;

            // Read header
            byte[] headerBytes;
            try
            {
                headerBytes = ReadHeader();
            }
            catch (PacketParsingException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.IncompleteHeader,
                    ex.Message,
                    packetStart));
                continue;
            }

            // Parse header
            if (!TryParseHeader(headerBytes, out uint time, out ushort devId, out SensorType typeId, out byte sourceId, out ushort size))
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.InvalidHeaderFormat,
                    "Неверный формат заголовка",
                    packetStart));
                continue;
            }

            // Validate packet size
            try
            {
                if (size == 0 || size > MaxPacketSize)
                    throw new InvalidOperationException($"Unsupported size: {size}");
                int expected = SensorDataFactory.GetExpectedLength(typeId);
                if (size != expected)
                    throw new PacketParsingException($"Size mismatch for {typeId}. Expected: {expected}, Actual: {size}");
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.SizeMismatch,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                // no content to skip
                continue;
            }
            catch (PacketParsingException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.UnknownSensorType,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                continue;
            }

            // Read content
            byte[] content;
            try
            {
                content = ReadContent(size);
            }
            catch (PacketParsingException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.ContentReadFailed,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                SkipInvalidPacket(size);
                continue;
            }

            // Skip padding
            int padding;
            try
            {
                padding = SkipPadding(size);
            }
            catch (PacketParsingException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.PaddingReadFailed,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                SkipInvalidPacket(size);
                continue;
            }

            // Validate checksum
            try
            {
                ValidateChecksum(headerBytes, content, size, padding);
            }
            catch (PacketParsingException ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.ChecksumMismatch,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                SkipInvalidPacket(size);
                continue;
            }

            // Create telemetry packet
            try
            {
                var parser = SensorDataFactory.CreateParser(typeId);
                parser.Parse(content);
                var packet = new TelemetryPacket(time, devId, typeId, sourceId, content, parser);
                packets.Add(packet);
            }
            catch (Exception ex)
            {
                errors.Add(new ParsingError(
                    _stream.Position,
                    ParsingErrorType.DataValidationFailed,
                    ex.Message,
                    packetStart,
                    devId,
                    typeId));
                SkipInvalidPacket(size);
                continue;
            }
        }

        return new PacketParsingResult(packets, errors);
    }

    private long FindSyncMarker()
    {
        int b;
        while ((b = _stream.ReadByte()) != -1)
        {
            if (b == _syncMarkerBytes[0])
            {
                long pos = _stream.Position - 1;
                var buf = new byte[PacketConstants.SyncMarkerLength];
                buf[0] = (byte)b;
                int read = _stream.Read(buf, 1, PacketConstants.SyncMarkerLength - 1);
                if (read < PacketConstants.SyncMarkerLength - 1)
                {
                    _stream.Position = pos + 1;
                    continue;
                }

                if (buf.AsSpan().SequenceEqual(_syncMarkerBytes))
                    return pos;

                _stream.Position = pos + 1;
            }
        }
        return -1;
    }

    private byte[] ReadHeader()
    {
        var header = new byte[PacketStructure.HeaderLength];
        int read = _stream.Read(header, 0, header.Length);
        if (read != header.Length)
            throw new PacketParsingException($"Неполный заголовок. Ожидалось: {header.Length}, получено: {read}");
        return header;
    }

    public static bool TryParseHeader(byte[] data, out uint time, out ushort devId, out SensorType type, out byte sourceId, out ushort size)
    {
        if (data.Length < PacketStructure.HeaderLength)
        {
            time = 0; devId = 0; type = default; sourceId = 0; size = 0;
            return false;
        }
        time = BinaryPrimitives.ReadUInt32BigEndian(data);
        devId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4));
        type = (SensorType)data[6];
        sourceId = data[7];
        size = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(8));
        return true;
    }

    private byte[] ReadContent(int size)
    {
        var buffer = new byte[size];
        int total = 0;
        while (total < size)
        {
            int r = _stream.Read(buffer, total, size - total);
            if (r == 0)
                throw new PacketParsingException("Не удалось прочитать содержимое");
            total += r;
        }
        return buffer;
    }

    private int SkipPadding(int contentSize)
    {
        int pad = PacketStructure.CalculatePadding(contentSize);
        if (pad > 0 && _stream.ReadByte() == -1)
            throw new PacketParsingException("Ошибка чтения выравнивающих байтов");
        return pad;
    }

    private void ValidateChecksum(byte[] header, byte[] content, int contentSize, int paddingSize)
    {
        var data = PacketStructure.CombineArrays(header, content, new byte[paddingSize]);
        var csBytes = new byte[2];
        if (_stream.Read(csBytes, 0, 2) != 2)
            throw new PacketParsingException("Ошибка чтения контрольной суммы");
        ushort expected = BinaryPrimitives.ReadUInt16BigEndian(csBytes);
        ushort actual = ChecksumCalculator.Compute(data);
        if (expected != actual)
            throw new PacketParsingException($"Ошибка контрольной суммы. Ожидалось: {expected}, получено: {actual}");
    }

    private void SkipInvalidPacket(int contentSize)
    {
        int pad = contentSize % 2;
        long skip = contentSize + pad + 2;
        if (_stream.Position + skip <= _stream.Length)
            _stream.Position += skip;
    }
}

public class PacketParsingException : Exception
{
    public PacketParsingException(string message) : base(message) { }
    public PacketParsingException(string message, Exception inner) : base(message, inner) { }
}
