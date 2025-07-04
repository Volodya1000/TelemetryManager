namespace TelemetryManager.Application.OutputDtos;

public record TelemetryPacketDto(
        DateTime DateTimeOfSending,
        ushort DevId,
        byte TypeId,
        byte SourceId,
        IReadOnlyList<PacketParameterDto> PacketParameters
    )
{
    public bool IsValid => PacketParameters.All( p => p.IsValid );
}
