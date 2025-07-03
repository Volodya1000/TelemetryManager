namespace TelemetryManager.Application.OutputDtos;

public record ParameterIntervalDto(
 double CurrentMin,
 double CurrentMax,
 string Unit,
 string Quantity
);
