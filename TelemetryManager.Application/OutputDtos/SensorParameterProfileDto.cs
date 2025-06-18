namespace TelemetryManager.Application.OutputDtos;

public sealed record SensorParameterProfileDto(
    string ParameterName,
    string Units,
    double MinValue,
    double MaxValue);