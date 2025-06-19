namespace TelemetryManager.Application.OutputDtos;

public sealed record SensorProfileDto(
    byte SourceId,
    byte TypeId,
    string Name,
    IReadOnlyList<SensorParameterProfileDto> Parameters);
