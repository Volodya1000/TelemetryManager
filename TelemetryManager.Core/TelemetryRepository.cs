using System.Collections.Generic;
using System.Collections.ObjectModel;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Core;

public class TelemetryRepository
{
    private readonly Dictionary<SensorId, SensorProfile> _sensors;
    private readonly Dictionary<SensorId, SensorHistoryRecord> _history;


    public void AddSensorSnapshot
}
