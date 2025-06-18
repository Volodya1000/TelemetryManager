using System.Collections.Generic;
using System.Linq;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Logger;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Services;

public class TelemtryService
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConfigurationValidator _configurationValidator;
    private readonly IPacketStreamParser _parser;
    private readonly IErrorLogger _errorLogger;

    //  private CancellationTokenSource? _cts;

    private List<DeviceProfile> deviceProfiles;

    private readonly List<TelemetryPacket> recivedPackets = new List<TelemetryPacket>();
    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();


    public TelemtryService(
         IConfigurationLoader configurationLoader,
         IConfigurationValidator configurationValidator,
         IPacketStreamParser parser,
         IErrorLogger errorLogger)
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
        _parser= parser;
        _errorLogger = errorLogger;
    }
    public void LoadConfiguration(string configFilePath)
    {
        deviceProfiles = _configurationLoader.Load(configFilePath);
        _configurationValidator.Validate(deviceProfiles);
    }

    public void ProcessTelemetryFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist.", filePath);

        using (var stream = File.OpenRead(filePath))
        {
            var parsingResult = _parser.Parse(stream, GetAvailableDeviceIdsWithSensorIds());
            recivedPackets.AddRange(parsingResult.Packets);
            parsingErrors.AddRange(parsingResult.Errors);
            SetActivationTimeForDevices(parsingResult.Packets);
        }
    }

    public List<DeviceProfile> GetDevicesProfiles() => deviceProfiles;

    public List<TelemetryPacket> GetRecivedPackets() => recivedPackets;

    public List<ParsingError> GetParsingErrors() => parsingErrors;

    private Dictionary<ushort, IReadOnlyCollection<SensorId>> GetAvailableDeviceIdsWithSensorIds()=>
        deviceProfiles.ToDictionary(d => d.DeviceId, d => d.SensorIds);


    /// <summary>
    /// Устанавливает время активации для устройств, у которых оно еще не установлено.
    /// Время активации вычисляется на основе самого раннего телеметрического пакета каждого устройства.
    /// </summary>
    /// <param name="receivedPackets">Коллекция полученных телеметрических пакетов</param>
    private void SetActivationTimeForDevices(IReadOnlyCollection<TelemetryPacket> receivedPackets)
    {
        if(receivedPackets.Count == 0) return;

        var devicesWithoutActivationTime = deviceProfiles
            .Where(d => !d.ActivationTime.HasValue)
            .ToList();

        if (!devicesWithoutActivationTime.Any())
            return;

        var packetsByDevice = receivedPackets
            .GroupBy(p => p.DevId)
            .ToDictionary(
                g => g.Key,
                g => g.MinBy(p => p.Time) // Находим пакет с минимальным временем для каждого устройства
            );

        foreach (var device in devicesWithoutActivationTime)
        {
            if (!packetsByDevice.TryGetValue(device.DeviceId, out var packet) || packet == null)
                continue;
            // Преобразуем миллисекунды в TimeSpan
            TimeSpan uptimeDuration = TimeSpan.FromMilliseconds(packet.Time);

            // Вычисляем время активации: текущее время минус продолжительность работы
            DateTime activationTime = DateTime.UtcNow - uptimeDuration;

            device.SetActivationTime(activationTime);
        }
    }

    public void UpdateSensorParametrMaxValue(ushort deviceId,SensorId SensorId,double newValue)
    {
        var sensor= deviceProfiles.Where(d => d.DeviceId==deviceId).FirstOrDefault();

    }


    //public void StartStream(Stream input)
    //{
    //    // Если предыдущий поток запущен, остановим его
    //    if (_cts != null)
    //    {
    //        StopStream();
    //    }

    //    _cts = new CancellationTokenSource();
    //    var token = _cts.Token;

    //    Task.Run(() =>
    //    {
    //        try
    //        {
    //            var reader = new PacketStreamParser(input);
    //            while (!token.IsCancellationRequested)
    //            {
    //                var raw = reader.ReadNextPacket();
    //                if (raw == null)
    //                    continue; // шум или EOF

    //                //if (!_parser.TryParse(raw, out var packet))
    //                //{
    //                //    OnPacketRejected("Parse error", raw.ToArray());
    //                //    continue;
    //                //}

    //                //var data = SensorDataDecoder.Decode(packet);
    //                //_storage.AddData(data);
    //                //OnSensorDataReceived(data);




    //                //if (_validator.IsAnomalous(data, out var anomalyInfo))
    //                //{
    //                //    _storage.AddAnomaly(anomalyInfo);
    //                //    OnAnomalyDetected(anomalyInfo);
    //                //}
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //           // _logger.Error("Stream processing failed", ex);
    //        }
    //    }, token);
    //}

    //public void StopStream()
    //{
    //    if (_cts == null) return;
    //    _cts.Cancel();
    //    _cts = null;
    //}




    //public event EventHandler<SensorDataEventArgs>? SensorDataReceived;
    //public event EventHandler<AnomalyEventArgs>? AnomalyDetected;



    //protected virtual void OnSensorDataReceived(SensorDataEventArgs e)
    //=> SensorDataReceived?.Invoke(this, e);


    //protected virtual void OnAnomalyDetected(AnomalyEventArgs e)
    //   => AnomalyDetected?.Invoke(this, e);

}
