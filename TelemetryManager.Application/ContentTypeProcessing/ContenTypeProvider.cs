using System.Collections.Concurrent;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Application.ContentTypeRegistration;

/// <summary>
/// Реестр определений типов сенсоров (SensorType → ContentDefinition),
/// отвечающий за регистрацию, проверку уникальности, доступ по typeId,
/// и предоставление полной информации о параметрах контента.
/// Используется для парсинга и валидации телеметрических данных.
/// </summary>
//public class ContenTypeProvider: IContentTypeProvider
//{
//    private readonly ConcurrentDictionary<byte, ContentDefinition> _defs = new();

//    public void Register(ContentDefinition definition)
//    {
//        if (!_defs.TryAdd(definition.TypeId, definition))
//            throw new InvalidOperationException($"TypeId {definition.TypeId} уже зарегистрирован");
//    }

//    public bool IsRegistered(byte typeId) => _defs.ContainsKey(typeId);

//    public ContentDefinition GetDefinition(byte typeId) =>
//        _defs.TryGetValue(typeId, out var def) ? def : throw new KeyNotFoundException($"TypeId {typeId} не найден");

//    public string GetName(byte typeId) => GetDefinition(typeId).Name.Value;

//    public IEnumerable<ContentDefinition> AllDefinitions => _defs.Values;
//}