using Microsoft.Extensions.Logging;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Infrastructure.Parsing.ParserStates;

class ValidationState : IParserState
{
    public async Task<IParserState> HandleAsync(ParserEnvironment env, PacketState state, CancellationToken ct)
    {
        try
        {
            if (!env.AvailableSensors.TryGetValue(state.DeviceId, out var sen))
                throw new KeyNotFoundException($"Device {state.DeviceId} not registered");

            var sid = new SensorId(state.TypeId, state.SourceId);
            if (!sen.Contains(sid))
                throw new InvalidDataException($"Sensor {sid} not in device {state.DeviceId}");

            var def = await env.DefinitionRepo.GetDefinitionAsync(state.TypeId);
            if (state.Size == 0 || state.Size > PacketStreamParser.MaxPacketSize
                || state.Size != def.TotalSizeBytes)
                throw new InvalidDataException($"Invalid size {state.Size} for type {state.TypeId}");

            return new ContentState();
        }
        catch (Exception ex)
        {
            env.Logger.LogError(ex, "Validation error at {Offset}", state.PacketStart);
            return new SyncState();
        }
    }
}
