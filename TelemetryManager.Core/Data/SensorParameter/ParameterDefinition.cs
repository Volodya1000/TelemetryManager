using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Data.SensorParameter;

public class ParameterDefinition
{
    public string Name { get; }
    public string Quantity { get; }
    public string Unit { get; }
    public Type DataType { get; }
    public IDataTypeHandler Handler { get; }

    public ParameterDefinition(string name, string quantity, string unit, Type dataType)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
        DataType = dataType;
        Handler = DataTypeHandlerRegistry.GetHandler(dataType);
    }

    public int ByteSize => Handler.GetSize();
}





