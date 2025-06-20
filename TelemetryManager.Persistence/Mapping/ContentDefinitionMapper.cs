using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Persistence.Entities;
using TelemetryManager.Persistence.Entities.ContentEntities;

namespace TelemetryManager.Persistence.Mapping;

public static class ContentDefinitionMapper
{
    public static ContentDefinition ToDomain(ContentDefinitionEntity entity)
    {
        var parameters = entity.Parameters
            .Select(p => new ParameterDefinition(
                new ParameterName(p.Name),
                p.Quantity,
                p.Unit,
                Type.GetType(p.DataTypeName)!)) // Восстанавливаем Type из строки
            .ToList();

        return new ContentDefinition(
            entity.TypeId,
            new Name(entity.Name),
            parameters);
    }

    public static ContentDefinitionEntity ToEntity(ContentDefinition domain)
    {
        var parameters = domain.Parameters
            .Select(p => new ParameterDefinitionEntity(
                name: p.Name.Value,
                quantity: p.Quantity,
                unit: p.Unit,
                dataTypeName: p.DataType.AssemblyQualifiedName!, // Полное имя типа
                contentDefinitionTypeId: domain.TypeId))
            .ToList();

        var entity = new ContentDefinitionEntity(
            typeId: domain.TypeId,
            name: domain.Name.Value,
            parameters: parameters);

        return entity;
    }
}