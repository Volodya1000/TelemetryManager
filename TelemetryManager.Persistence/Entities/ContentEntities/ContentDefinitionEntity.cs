namespace TelemetryManager.Persistence.Entities.ContentEntities;

public class ContentDefinitionEntity
{
    public byte TypeId { get; private set; }
    public string Name { get; private set; }
    public List<ParameterDefinitionEntity> Parameters { get; private set; }

    // Приватный конструктор для EF
    private ContentDefinitionEntity() { }

    public ContentDefinitionEntity(byte typeId, string name, List<ParameterDefinitionEntity> parameters)
    {
        TypeId = typeId;
        Name = name;
        Parameters = parameters;
    }
}
