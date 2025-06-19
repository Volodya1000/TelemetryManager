using System.Threading.Tasks;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Mapping;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Application.Requests;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.EventsArgs;
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
    private readonly ParameterValidationService _parameterValidationService;
    private readonly IFileReaderService _fileReader;


    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();

    public event EventHandler<ParameterOutOfRangeEventArgs>? ParameterOutOfRange;


    public TelemetryProcessingService(
        IConfigurationLoader configurationLoader,
        IConfigurationValidator configurationValidator,
        IPacketStreamParser parser,
        ITelemetryRepository telemetryRepository,
        DeviceService deviceService,
        IFileReaderService fileReader,
        ParameterValidationService parameterValidationService) 
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
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
        var parsingResult = _parser.Parse(stream, deviceSensorsIdsDictionary);

        var baseTime = DateTime.UtcNow;

        // Получаем устройства без активации
        var devicesWithoutActivation = (await _deviceService.GetDevicesWithoutActivationTimeAsync())
            .ToHashSet();

        // Создаем финальные пакеты и находим минимальное время для активации
        var telemetryPackets = new List<TelemetryPacket>();
        var minUptimes = new Dictionary<ushort, long>();

        foreach (var packet in parsingResult.Packets)
        {
            // Вычисляем время отправки
            var uptimeDuration = TimeSpan.FromMilliseconds(packet.Time);
            var sendTime = baseTime + uptimeDuration;
            telemetryPackets.Add(new TelemetryPacket(sendTime, packet.DevId, packet.SensorId, packet.Content));

            // Обновляем минимальное время для активации
            if (devicesWithoutActivation.Contains(packet.DevId))
            {
                if (!minUptimes.TryGetValue(packet.DevId, out var currentMin) || packet.Time < currentMin)
                {
                    minUptimes[packet.DevId] = packet.Time;
                }
            }
        }

        // Устанавливаем время активации
        foreach (var (deviceId, minUptime) in minUptimes)
        {
            var activationTime = baseTime - TimeSpan.FromMilliseconds(minUptime);
            await _deviceService.SetActivationTimeAsync(deviceId, activationTime);
        }

        // Обрабатываем ошибки
        parsingErrors.AddRange(parsingResult.Errors);

        // Сохраняем пакеты
        foreach (var packet in telemetryPackets)
        {
            foreach (var parameter in packet.Content)
            {
                await _parameterValidationService.ValidateAsync(
                    packet.DevId,
                    packet.SensorId,
                    parameter.Key,
                    parameter.Value
                );
            }

            await _telemetryRepository.AddPacketAsync(packet);
        }
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
