using System.Buffers.Binary;
using System.IO;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Core.Utils;

namespace TelemetryManager.Infrastructure.Parsing;

public class PacketStreamParser: IPacketStreamParser
{
    private const int MaxPacketSize = 1024;
    private  Stream _stream;
    private readonly byte[] _syncMarkerBytes = PacketConstants.SyncMarkerBytes;

    

    public List<TelemetryPacket> Parse(Stream stream)
    {
        _stream= stream;
        var packets = new List<TelemetryPacket>();

        while (true)
        {
            //Ищем позицию маркера
            long syncPosition = FindSyncMarker();
            if (syncPosition == -1) break;

            //Пропускаем маркер
            _stream.Position = syncPosition + PacketConstants.SyncMarkerLength; 

            //Чтение заголовка
            byte[] headerBytes = ReadHeader();

            var headerInfo = ParseHeader(headerBytes);
            ValidatePacketSize(headerInfo.Size, headerInfo.TypeId);

            // Чтение содержимого пакета
            byte[] content = ReadContent(headerInfo.Size);

            // Пропуск выравнивающих байтов
            int paddingSize=SkipPadding(headerInfo.Size);

            // Проверка контрольной суммы
            ValidateChecksum(headerBytes, content, headerInfo.Size, paddingSize);

            // Создание и сохранение телеметрического пакета
            TelemetryPacket packet = CreateTelemetryPacket(headerInfo, content);
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

    private byte[] ReadHeader()
    {
        byte[] headerBytes = new byte[PacketStructure.HeaderLength];
        int bytesRead = _stream.Read(headerBytes, 0, PacketStructure.HeaderLength);

        if (bytesRead != headerBytes.Length)
            throw new PacketParsingException(
                $"Неполный заголовок. Ожидалось: {headerBytes.Length}, получено: {bytesRead}");

        return headerBytes;
    }

    private (uint Time, ushort DevId, SensorType TypeId, byte SourceId, ushort Size)
       ParseHeader(byte[] headerBytes)
    {
        if (!TryParseHeader(
            headerBytes,
            out uint time,
            out ushort devId,
            out SensorType typeId,
            out byte sourceId,
            out ushort size))
        {
            throw new PacketParsingException("Неверный формат заголовка");
        }

        return (time, devId, typeId, sourceId, size);
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
        byte[] content = new byte[size];
        int totalRead = 0;

        while (totalRead < size)
        {
            int bytesRead = _stream.Read(content, totalRead, size - totalRead);
            if (bytesRead == 0)
                throw new PacketParsingException("Не удалось прочитать содержимое пакета");

            totalRead += bytesRead;
        }

        return content;
    }

    public static bool TryParseHeader(byte[] data, out uint time, out ushort devId, out SensorType type, out byte sourceId, out ushort size)
    {
        if (data.Length < PacketStructure.HeaderLength)
        {
            time = 0; devId = 0; type = 0; sourceId = 0; size = 0;
            return false;
        }

        time = (uint)(
            (data[0] << 24) |
            (data[1] << 16) |
            (data[2] << 8) |
            data[3]
        );

        devId = (ushort)(
            (data[4] << 8) |
            data[5]
        );

        type = (SensorType)data[6];
        sourceId = data[7];

        size = (ushort)(
            (data[8] << 8) |
            data[9]
        );

        return true;
    }
    private void ValidatePacketSize(ushort size,SensorType typeId)
    {
        if (size <= 0 || size > MaxPacketSize)
            throw new InvalidOperationException($"Unsuported sixe: {size}");
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
    }

    private int SkipPadding(int contentSize)
    {
        int paddingSize = PacketStructure.CalculatePadding(contentSize);
        if (paddingSize > 0 && _stream.ReadByte() == -1)
            throw new PacketParsingException("Ошибка чтения выравнивающих байтов");
        return paddingSize;
    }

    private void ValidateChecksum(byte[] headerBytes, byte[] content, int contentSize,int paddingSize)
    {
        byte[] dataForChecksum = PacketStructure.CombineArrays(
            headerBytes,
            content,
            new byte[paddingSize]
        );

        // Чтение контрольной суммы из потока
        byte[] csBytes = new byte[2];
        if (_stream.Read(csBytes, 0, 2) != 2)
            throw new PacketParsingException("Ошибка чтения контрольной суммы");

        ushort expectedCs = BinaryPrimitives.ReadUInt16BigEndian(csBytes);
        ushort actualCs = ChecksumCalculator.Compute(dataForChecksum);

        if (expectedCs != actualCs)
            throw new PacketParsingException(
                $"Ошибка контрольной суммы. Ожидалось: {expectedCs}, получено: {actualCs}");
    }

    private TelemetryPacket CreateTelemetryPacket(
        (uint Time, ushort DevId, SensorType TypeId, byte SourceId, ushort Size) headerInfo,
        byte[] content)
    {
        try
        {
            ISensorData sensorData = SensorDataFactory.CreateParser(headerInfo.TypeId);
            sensorData.Parse(content);

            return new TelemetryPacket(
                headerInfo.Time,
                headerInfo.DevId,
                headerInfo.TypeId,
                headerInfo.SourceId,
                content,
                sensorData
            );
        }
        catch (Exception ex)
        {
            throw new PacketParsingException(
                $"Ошибка создания данных сенсора: {ex.Message}", ex);
        }
    }
}

public class PacketParsingException : Exception
{
    public PacketParsingException(string message) : base(message) { }
    public PacketParsingException(string message, Exception innerException) : base(message, innerException) { }
}