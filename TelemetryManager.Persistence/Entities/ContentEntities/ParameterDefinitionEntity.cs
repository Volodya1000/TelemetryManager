namespace TelemetryManager.Persistence.Entities.ContentEntities;

public class ParameterDefinitionEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Quantity { get; private set; }
    public string Unit { get; private set; }
    public string DataTypeName { get; private set; }

    public byte ContentDefinitionTypeId { get; private set; }
    public ContentDefinitionEntity ContentDefinition { get; private set; }

    private ParameterDefinitionEntity() { }

    public ParameterDefinitionEntity(
      string name,
      string quantity,
      string unit,
      string dataTypeName,
      byte contentDefinitionTypeId)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
        DataTypeName = dataTypeName;
        ContentDefinitionTypeId = contentDefinitionTypeId;
    }
}