namespace TelemetryManager.Core.Exceptions;

public class ParameterValidationExceptions: Exception
{
    public string ParameterName { get; }
    public string Message { get; }
    public double? Value { get; }
    public double? MinValue { get; }
    public double? MaxValue { get; }

    public ParameterValidationExceptions(string parameterName, string message, double? value = null, double? minValue = null, double? maxValue = null)
    {
        ParameterName = parameterName;
        Message = message;
        Value = value;
        MinValue = minValue;
        MaxValue = maxValue;
    }
}