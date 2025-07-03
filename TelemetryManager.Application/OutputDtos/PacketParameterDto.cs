namespace TelemetryManager.Application.OutputDtos;

public record PacketParameterDto(
     string ParameterName,
     double value,
     bool InInterval,
     ParameterIntervalDto ParameterInterval
     );
