using System.Threading.Tasks;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Mapping;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class TelemetryProcessingService
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConfigurationValidator _configurationValidator;
    private readonly IPacketStreamParser _parser;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly DeviceService _deviceService;
    private readonly IFileReaderService _fileReader;


    //  private CancellationTokenSource? _cts;


    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();


   
    public TelemetryProcessingService(
        IConfigurationLoader configurationLoader,
        IConfigurationValidator configurationValidator,
        IPacketStreamParser parser,
        ITelemetryRepository telemetryRepository,
        DeviceService deviceService,
        IFileReaderService fileReader) 
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
        _parser = parser;
        _telemetryRepository = telemetryRepository;
        _deviceService = deviceService;
        _fileReader = fileReader;
    }

    //public void LoadConfiguration(string configFilePath)
    //{
    //    deviceProfiles = _configurationLoader.Load(configFilePath);
    //    _configurationValidator.Validate(deviceProfiles);
    //}

    public async Task ProcessTelemetryFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist.", filePath);

        using var stream = File.OpenRead(filePath);
        var deviceSensorsIdsDictionary = await _deviceService.GetAllDeviceSensorsIdsDictionaryAsync();
        var parsingResult = _parser.Parse(stream, deviceSensorsIdsDictionary);

      


        await SetActivationTimeForDevicesAsync(parsingResult.Packets);

        var packets = parsingResult.Packets.Select(p =>
        {
            var uptimeDuration = TimeSpan.FromMilliseconds(p.Time);
            var sendTime = DateTime.UtcNow + uptimeDuration;
            return new TelemetryPacket(sendTime, p.DevId, p.SensorId, p.Content);
        });
   
        parsingErrors.AddRange(parsingResult.Errors);

        foreach (var packet in packets)
            await _telemetryRepository.AddPacketAsync(packet);
    }


    /// <summary>
    /// Устанавливает время активации для устройств, у которых оно еще не установлено.
    /// Время активации вычисляется на основе самого раннего телеметрического пакета каждого устройства.
    /// </summary>
    private async Task SetActivationTimeForDevicesAsync(
      IReadOnlyCollection<TelemetryPacketWithUIntTime> receivedPackets)
    {
        if (receivedPackets.Count == 0) return;

        // Получаем устройства без времени активации через сервис
        var devicesWithoutActivation = await _deviceService.GetDevicesWithoutActivationTimeAsync();

        if (!devicesWithoutActivation.Any()) return;

        var packetsByDevice = receivedPackets
            .GroupBy(p => p.DevId)
            .ToDictionary(
                g => g.Key,
                g => g.MinBy(p => p.Time)
            );

        foreach (var deviceId in devicesWithoutActivation)
        {
            if (!packetsByDevice.TryGetValue(deviceId, out var packet) || packet == null)
                continue;

            // Преобразуем миллисекунды в TimeSpan
            var uptimeDuration = TimeSpan.FromMilliseconds(packet.Time);
            // Вычисляем время активации: текущее время минус продолжительность работы
            var activationTime = DateTime.UtcNow - uptimeDuration;

            await _deviceService.SetActivationTimeAsync(deviceId, activationTime);
        }
    }


    public async Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(TelemetryPacketRequestFilter filter)
    {
        return  await _telemetryRepository.GetPacketsAsync(
            filter.DateFrom, filter.DateTo, filter.DeviceId,
            filter.SensorType, filter.SensorId, filter.PageNumber, filter.PageSize);
    }


    public List<ParsingError> GetParsingErrors() => parsingErrors;







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
