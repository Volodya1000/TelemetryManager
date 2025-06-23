using System.Buffers.Binary;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Core.Utils;
using TelemetryManager.Infrastructure.Parsing.Data;

namespace TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

public class TelemetryGenerator
{

    private readonly Random _random = new Random();

    private readonly IDeviceRepository _deviceRepository;
    private readonly ContentGenerator _contentGenerator;

    public TelemetryGenerator(IDeviceRepository deviceRepository, ContentGenerator contentGenerator)
    {
        _deviceRepository = deviceRepository;
        _contentGenerator= contentGenerator;
    }

    public async Task Generate(string filePath,int packetsCount, double noiseRatio=0)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create);
        for (int i = 0; i < packetsCount; i++)
        {
            GenerateNoise(fileStream, noiseRatio);
            var packet = await GenerateValidPacket();
            CorruptPacketIfNeeded(ref packet, noiseRatio);
            fileStream.Write(packet, 0, packet.Length);
        }
    }

    private async Task<byte[]> GenerateValidPacket()
    {
        var allDevices = (await _deviceRepository.GetAllAsync()).ToList();

        int randomDeviceIndex = _random.Next(allDevices.Count());

        var currentDevice = allDevices[randomDeviceIndex];

        int randomSensorIndex = _random.Next(currentDevice.Sensors.Count);

        SensorProfile currentSensor = currentDevice.Sensors.ElementAt(randomSensorIndex);

        byte[] content = await _contentGenerator.GenerateContentAsync(currentSensor,true);

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
         var currentTime=(uint)_random.Next(1, 101);
        return currentTime;
    }

    private void GenerateNoise(Stream stream,double noiseRatio)
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

