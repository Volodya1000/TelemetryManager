using TelemetryManager.Core.Enums;

namespace TelemetryManager.Application.OutputDtos;

public sealed record SensorProfileDto(
    byte SourceId,
    SensorType TypeId,
    string Name,
    IReadOnlyList<SensorParameterProfileDto> Parameters);
