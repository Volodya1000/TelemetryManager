namespace TelemetryManager.Core.Data.ValueObjects;

public sealed record ParameterName : StringValueObject
{
    public const int MIN_LENGTH = 1;
    public const int MAX_LENGTH =20;

    public ParameterName(string value) : base(value, MIN_LENGTH, MAX_LENGTH) { }
}