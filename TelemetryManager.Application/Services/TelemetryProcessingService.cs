using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Requests;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.EventsArgs;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class TelemetryProcessingService
{
    //private readonly IConfigurationLoader _configurationLoader;
    //private readonly IConfigurationValidator _configurationValidator;
    private readonly IPacketStreamParser _parser;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly DeviceService _deviceService;
    private readonly ParameterValidationService _parameterValidationService;
    private readonly IFileReaderService _fileReader;


    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();

    public event EventHandler<ParameterOutOfRangeEventArgs>? ParameterOutOfRange;


    public TelemetryProcessingService(
        //IConfigurationLoader configurationLoader,
        //IConfigurationValidator configurationValidator,
        IPacketStreamParser parser,
        ITelemetryRepository telemetryRepository,
        DeviceService deviceService,
        IFileReaderService fileReader,
        ParameterValidationService parameterValidationService) 
    {
    //    _configurationLoader = configurationLoader;
    //    _configurationValidator = configurationValidator;
        _parser = parser;
        _telemetryRepository = telemetryRepository;
        _deviceService = deviceService;
        _fileReader = fileReader;
        _parameterValidationService = parameterValidationService;

        _parameterValidationService.ParameterOutOfRange += (sender, args) =>
            ParameterOutOfRange?.Invoke(sender, args);
    }

    public async Task ProcessTelemetryStream(Stream stream)
    {
        var deviceSensorsIdsDictionary = await _deviceService.GetAllDeviceSensorsIdsDictionaryAsync();
        var devicesNeedingActivation = (await _deviceService.GetDevicesWithoutActivationTimeAsync())
            .ToHashSet();

        var baseTime = DateTime.UtcNow;
        var parsingErrors = new List<ParsingError>();

        await foreach (var packet in _parser.Parse(stream, deviceSensorsIdsDictionary,
            error => parsingErrors.Add(error)))
        {
            var sendTime = baseTime + TimeSpan.FromMilliseconds(packet.Time);

            bool isCurrentlyConnected = await _deviceService.IsSensorCurrentlyConnectedAsync(
                packet.DevId,
                packet.SensorId.TypeId,
                packet.SensorId.SourceId);

            if (isCurrentlyConnected)
            {
                // Обработка активации (только для первого пакета устройства)
                if (devicesNeedingActivation.Contains(packet.DevId))
                {
                    var activationTime = sendTime - TimeSpan.FromMilliseconds(packet.Time);
                    await _deviceService.SetActivationTimeAsync(packet.DevId, activationTime);
                    devicesNeedingActivation.Remove(packet.DevId); // Удаляем из множества нуждающихся
                }

                // Валидация и сохранение пакета
                var telemetryPacket = new TelemetryPacket(sendTime, packet.DevId, packet.SensorId, packet.Content);
                foreach (var parameter in packet.Content)
                {
                    await _parameterValidationService.ValidateAsync(
                        packet.DevId,
                        packet.SensorId,
                        parameter.Key,
                        parameter.Value);
                }
                await _telemetryRepository.AddPacketAsync(telemetryPacket);
            }
        }

        //if (parsingErrors.Count > 0)
        //{
        //    await _errorRepository.SaveErrorsAsync(parsingErrors);
        //}
    }

    public async Task ProcessTelemetryFile(string filePath)
    {
        await using var stream = _fileReader.OpenRead(filePath);

        await ProcessTelemetryStream(stream);
    }
 

    public async Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(TelemetryPacketFilterRequest filter)
    {
        return  await _telemetryRepository.GetPacketsAsync(
            filter.DateFrom, filter.DateTo, filter.DeviceId,
            filter.SensorType, filter.SensorId, filter.PageNumber, filter.PageSize);
    }


    public List<ParsingError> GetParsingErrors() => parsingErrors;
}
