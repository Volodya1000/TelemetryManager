using TelemetryManager.Persistence.Entities.DeviceEntities;

namespace TelemetryManager.Persistence.Entities.ContentEntities;

public class ParameterDefinitionEntity
{
    public byte TypeId { get; set; }
    public string Name { get; set; }

    public string Quantity { get; set; }
    public string Unit { get; set; }
    public string DataTypeName { get; set; }

    public List<SensorParameterProfileEntity> SensorParameters { get; set; }
        = new();

    public ContentDefinitionEntity ContentDefinition { get; set; }
}