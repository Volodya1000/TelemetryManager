namespace TelemetryManager.Core.Interfaces;

public interface IDataTypeHandler
{
    int GetSize();
    object ParseValue(ReadOnlySpan<byte> data);
    double ConvertToDouble(object value);
}
