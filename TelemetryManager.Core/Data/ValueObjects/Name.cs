namespace TelemetryManager.Core.Data.ValueObjects;

public sealed record Name : StringValueObject
{
    public const int MIN_LENGTH = 5;
    public const int MAX_LENGTH = 40;

    public Name(string value) : base(value, MIN_LENGTH, MAX_LENGTH) { }
}
