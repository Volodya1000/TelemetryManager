namespace TelemetryManager.Core.Data.ValueObjects;

public sealed record ParametrName : StringValueObject
{
    public const int MIN_LENGTH = 1;
    public const int MAX_LENGTH =20;

    public ParametrName(string value) : base(value, MIN_LENGTH, MAX_LENGTH) { }
}