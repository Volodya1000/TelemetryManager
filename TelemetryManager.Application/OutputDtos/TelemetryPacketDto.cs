using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.OutputDtos;

public record TelemetryPacketDto(
        DateTime DateTimeOfSending,
        ushort DevId,
        byte TypeId,
        byte SourceId,
        IReadOnlyList<PacketParameterDto> PacketParameters
    );
