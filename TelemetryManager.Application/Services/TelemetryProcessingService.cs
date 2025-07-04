using System;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Application.Requests;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.EventsArgs;
using TelemetryManager.Core.Identifiers;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class TelemetryProcessingService
{
    private readonly IPacketStreamParser _parser;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly DeviceService _deviceService;
    private readonly ParameterValidationService _parameterValidationService;


    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();

    public event EventHandler<ParameterOutOfRangeEventArgs>? ParameterOutOfRange;


    public TelemetryProcessingService(
        IPacketStreamParser parser,
        ITelemetryRepository telemetryRepository,
        DeviceService deviceService,
        ParameterValidationService parameterValidationService) 
    {
        _parser = parser;
        _telemetryRepository = telemetryRepository;
        _deviceService = deviceService;
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
                        parameter.Value,
                        sendTime);
                }
                await _telemetryRepository.AddPacketAsync(telemetryPacket);
            }
        }

        //if (parsingErrors.Count > 0)
        //{
        //    await _errorRepository.SaveErrorsAsync(parsingErrors);
        //}
    }


    public async Task<PagedResponse<TelemetryPacket>> GetPacketsAsync(TelemetryPacketFilterRequest filter)
    {
        return  await _telemetryRepository.GetPacketsAsync(
            filter.DateFrom, filter.DateTo, filter.DeviceId,
            filter.SensorType, filter.SensorId, filter.PageNumber, filter.PageSize);
    }

    public async Task<PagedResponse<TelemetryPacketDto>> GetPacketsDetailedAsync(TelemetryPacketFilterRequest filter)
    {
        var packetsPagedResponse = await _telemetryRepository.GetPacketsAsync(
            filter.DateFrom, filter.DateTo, filter.DeviceId,
            filter.SensorType, filter.SensorId, filter.PageNumber, filter.PageSize);

        var packetsDtos = await Task.WhenAll(packetsPagedResponse.Data.Select(async packet =>
        {
            var parametersDtos = await Task.WhenAll(packet.Content.Select(async parameter =>
            {
                var (devId, sensorTypeId, sensorSourceId, dateTime) =
                    (packet.DevId, packet.SensorId.TypeId, packet.SensorId.SourceId, packet.DateTimeOfSending);

                var parameterIntervalTask = _deviceService.GetParameterInterval(
                    devId, sensorTypeId, sensorSourceId, parameter.Key, dateTime);

                var intervalValueTask = _deviceService.CheckParameterValue(
                    devId, sensorTypeId, sensorSourceId, parameter.Key, parameter.Value, dateTime);

                await Task.WhenAll(parameterIntervalTask, intervalValueTask);

                return new PacketParameterDto(
                    parameter.Key,
                    parameter.Value,
                    intervalValueTask.Result.isValid,
                    parameterIntervalTask.Result);
            }));

            return new TelemetryPacketDto(
                packet.DateTimeOfSending,
                packet.DevId,
                packet.SensorId.TypeId,
                packet.SensorId.SourceId,
                parametersDtos.ToList());
        }));

        return new PagedResponse<TelemetryPacketDto>(
            packetsDtos.ToList(),
            packetsPagedResponse.TotalRecords,
            packetsPagedResponse.PageNumber,
            packetsPagedResponse.PageSize);
    }

    public List<ParsingError> GetParsingErrors() => parsingErrors;
}
