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
        var packetsPagedResponce= await _telemetryRepository.GetPacketsAsync(
            filter.DateFrom, filter.DateTo, filter.DeviceId,
            filter.SensorType, filter.SensorId, filter.PageNumber, filter.PageSize);


        var packetsDtos = new List<TelemetryPacketDto>();
        foreach (var packet in packetsPagedResponce.Data)
        {
            var parametrsDtosList = new List<PacketParameterDto>();

            foreach(var parametr in packet.Content)
            {
                var parameterIntervalDto = await _deviceService.GetParameterInterval(packet.DevId,
                                                                                     packet.SensorId.TypeId,
                                                                                     packet.SensorId.SourceId,
                                                                                     parametr.Key,
                                                                                     packet.DateTimeOfSending);
                var intervalValue = await _deviceService.CheckParameterValue(packet.DevId,
                                                                      packet.SensorId.TypeId,
                                                                      packet.SensorId.SourceId,
                                                                      parametr.Key,
                                                                      parametr.Value,
                                                                      packet.DateTimeOfSending);


                var parametrDto = new PacketParameterDto(parametr.Key,
                                                         parametr.Value,
                                                         intervalValue.isValid,
                                                         parameterIntervalDto);
                parametrsDtosList.Add(parametrDto);
            }

            var packetDto = new TelemetryPacketDto(packet.DateTimeOfSending,
                                                   packet.DevId, 
                                                   packet.SensorId.TypeId,
                                                   packet.SensorId.SourceId,
                                                   parametrsDtosList);

            packetsDtos.Add(packetDto);
        }

        var pagedResponceDtos = new PagedResponse<TelemetryPacketDto>(packetsDtos,
                                                                packetsPagedResponce.TotalRecords,
                                                                packetsPagedResponce.PageNumber,
                                                                packetsPagedResponce.PageSize);

        return pagedResponceDtos;
    }

    public List<ParsingError> GetParsingErrors() => parsingErrors;
}
