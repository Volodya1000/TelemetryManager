using System.Collections.Concurrent;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Core.Data.SensorParameter;

namespace TelemetryManager.Application.Services;

public class DataTypeHandlerService: IDataTypeHandlerService
{
    private readonly ConcurrentDictionary<Type, IDataTypeHandler> _handlers = new();

    public DataTypeHandlerService()
    {
        RegisterHandler(typeof(bool), new BoolHandler());
        RegisterHandler(typeof(sbyte), new SByteHandler());
        RegisterHandler(typeof(byte), new ByteHandler());
        RegisterHandler(typeof(short), new ShortHandler());
        RegisterHandler(typeof(ushort), new UShortHandler());
        RegisterHandler(typeof(int), new IntHandler());
        RegisterHandler(typeof(uint), new UIntHandler());
        RegisterHandler(typeof(long), new LongHandler());
        RegisterHandler(typeof(ulong), new ULongHandler());
        RegisterHandler(typeof(float), new FloatHandler());
        RegisterHandler(typeof(double), new DoubleHandler());
        RegisterHandler(typeof(decimal), new DecimalHandler());
    }

    public void RegisterHandler(Type type, IDataTypeHandler handler)
    {
        if (!_handlers.TryAdd(type, handler))
            throw new InvalidOperationException($"Обработчик для типа {type.Name} уже зарегистрирован.");
    }

    public IDataTypeHandler GetHandler(Type type)
    {
        if (_handlers.TryGetValue(type, out var handler))
            return handler;

        throw new NotSupportedException($"Тип {type.Name} не поддерживается");
    }
}