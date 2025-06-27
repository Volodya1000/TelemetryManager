namespace TelemetryManager.Application.OutputDtos;

public sealed record DeviceDto(
    ushort DeviceId,
    string Name,
    DateTime? ActivationTime);
