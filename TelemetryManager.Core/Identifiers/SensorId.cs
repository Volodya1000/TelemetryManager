namespace TelemetryManager.Core.Identifiers;

public readonly record struct SensorId(ushort DeviceId, ushort TypeId, byte SourceId)
{
    public override string ToString()=>
         $"DeviceId={DeviceId}, Type={TypeId}, SourceId={SourceId}";
}
