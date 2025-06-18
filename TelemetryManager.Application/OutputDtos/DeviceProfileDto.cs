namespace TelemetryManager.Application.OutputDtos;

public sealed record DeviceProfileDto(
    ushort DeviceId,
    string Name,
    DateTime? ActivationTime,
    IReadOnlyList<SensorProfileDto> Sensors);
