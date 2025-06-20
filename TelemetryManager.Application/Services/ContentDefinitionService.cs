using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Interfaces.Repositories;

namespace TelemetryManager.Application.Services;

public class ContentDefinitionService : IContentDefinitionService
{
    private readonly IContentDefinitionRepository _contentDefinitionRepository;

    public ContentDefinitionService(IContentDefinitionRepository contentDefinitionRepository)
    {
        _contentDefinitionRepository = contentDefinitionRepository;
    }

    public async Task RegisterAsync(
        byte id,
        string contentName,
        IEnumerable<(string ParamName, string Description, string Unit, Type DataType)> parameters)
    {
        var parameterDefinitions = parameters.Select(p =>
            new ParameterDefinition(
                new ParameterName(p.ParamName),
                p.Description,
                p.Unit,
                p.DataType
            )).ToList();

        var contentDefinition = new ContentDefinition(
            id,
            new Name(contentName),
            parameterDefinitions
        );

        await _contentDefinitionRepository.RegisterAsync(contentDefinition);
    }
}