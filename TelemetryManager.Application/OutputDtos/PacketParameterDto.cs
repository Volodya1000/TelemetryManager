namespace TelemetryManager.Application.OutputDtos;

public record PacketParameterDto(
     string ParameterName,
     double Value,
     bool IsValid,
     ParameterIntervalDto ParameterInterval
     );
