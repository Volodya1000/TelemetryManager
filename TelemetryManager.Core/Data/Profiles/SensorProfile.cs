using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core.Data.Profiles;

public class SensorProfile
{
    public SensorId SensorId { get; }
    public string Name { get; }
    public IReadOnlyList<SensorParameterProfile> Parameters { get; }

    public SensorProfile(SensorId sensorId, string name, IEnumerable<SensorParameterProfile> parameters)
    {
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
    }
}