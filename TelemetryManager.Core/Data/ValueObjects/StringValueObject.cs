using System.Text.RegularExpressions;

namespace TelemetryManager.Core.Data.ValueObjects;

public abstract record StringValueObject(int MinLength, int MaxLength)
{
    protected Regex ValidationRegex { get; } = CreateDefaultRegex(MinLength, MaxLength);

    public string Value { get; }

    protected StringValueObject(string value, int minLength, int maxLength)
        : this(minLength, maxLength)
    {
        if (!IsValid(value))
            throw new ArgumentException($"Value must be between {MinLength} and {MaxLength} characters", nameof(value));

        Value = value;
    }

    public virtual bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Length >= MinLength &&
               value.Length <= MaxLength &&
               ValidationRegex.IsMatch(value);
    }

    public override string ToString() => Value;

    protected static Regex CreateDefaultRegex(int minLength, int maxLength) => new(
        $@"^[\p{{L}}\p{{M}}\p{{N}}]{{{minLength},{maxLength}}}$",
        RegexOptions.Singleline | RegexOptions.Compiled);

}
