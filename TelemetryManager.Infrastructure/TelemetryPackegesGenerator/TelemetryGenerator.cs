using System.Buffers.Binary;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Core.Utils;
using TelemetryManager.Infrastructure.Parsing.Data;

namespace TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

public class TelemetryGenerator : ITelemetryGenerator
{
    private readonly Random _random = new Random();

    private readonly IDeviceRepository _deviceRepository;
    private readonly ContentGenerator _contentGenerator;

    public TelemetryGenerator(IDeviceRepository deviceRepository, ContentGenerator contentGenerator)
    {
        _deviceRepository = deviceRepository;
        _contentGenerator = contentGenerator;
    }

    public async Task<byte[]> GeneratePackets(int packetsCount, double noiseRatio = 0, double validityRatio = 1.0)
    {
        if (packetsCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(packetsCount), "Количество пакетов должно быть больше нуля");

        if (noiseRatio < 0 || noiseRatio > 1)
            throw new ArgumentOutOfRangeException(nameof(noiseRatio), "Вероятность шума должна быть в диапазоне от 0 до 1");

        if (validityRatio < 0 || validityRatio > 1)
            throw new ArgumentOutOfRangeException(nameof(validityRatio), "Вероятность валидности должна быть в диапазоне от 0 до 1");

        using var memoryStream = new MemoryStream();
        for (int i = 0; i < packetsCount; i++)
        {
            GenerateNoise(memoryStream, noiseRatio);
            bool shouldBeValid = _random.NextDouble() < validityRatio;
            var packet = await GeneratePacket(shouldBeValid);

            CorruptPacketIfNeeded(ref packet, noiseRatio);
            memoryStream.Write(packet, 0, packet.Length);
        }
        return memoryStream.ToArray();
    }

    private async Task<byte[]> GeneratePacket(bool shouldBeValid)
    {
        var allDevices = (await _deviceRepository.GetAllAsync()).ToList();

        int randomDeviceIndex = _random.Next(allDevices.Count());

        var currentDevice = allDevices[randomDeviceIndex];

        int randomSensorIndex = _random.Next(currentDevice.Sensors.Count);

        SensorProfile currentSensor = currentDevice.Sensors.ElementAt(randomSensorIndex);

        byte[] content = await _contentGenerator.GenerateContentAsync(currentSensor, shouldBeValid);

        uint time = DefaultTimeGenerator();
        int paddingSize = PacketHelper.CalculatePadding(content.Length);

        byte[] headerBytes = PacketHelper.BuildHeaderBytes(
            time,
            currentDevice.DeviceId,
            currentSensor.TypeId,
            currentSensor.SourceId,
            (ushort)content.Length
        );

        using var ms = new MemoryStream();
        ms.Write(PacketConstants.SyncMarkerBytes, 0, PacketConstants.SyncMarkerLength);
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

    private uint DefaultTimeGenerator()
    {
        var currentTime = (uint)_random.Next(1, 101);
        return currentTime;
    }

    private void GenerateNoise(Stream stream, double noiseRatio)
    {
        if (_random.NextDouble() >= noiseRatio) return;

        var noise = new byte[_random.Next(1, 6)];
        _random.NextBytes(noise);
        stream.Write(noise, 0, noise.Length);
    }

    private void CorruptPacketIfNeeded(ref byte[] packet, double noiseRatio)
    {
        if (_random.NextDouble() >= noiseRatio) return;

        // Повреждаем случайный байт в пакете
        var index = _random.Next(packet.Length);
        packet[index] = (byte)_random.Next(256);
    }
}

