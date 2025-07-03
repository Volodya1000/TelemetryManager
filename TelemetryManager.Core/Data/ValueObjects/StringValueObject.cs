using System.Text.RegularExpressions;

public abstract record StringValueObject(int MinLength, int MaxLength)
{
    protected Regex ValidationRegex { get; } = CreateDefaultRegex(MinLength, MaxLength);

    public string Value { get; }

    protected StringValueObject(string value, int minLength, int maxLength)
        : this(minLength, maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));

        if (value.Length < MinLength || value.Length > MaxLength)
            throw new ArgumentException($"Value must be between {MinLength} and {MaxLength} characters", nameof(value));

        if (!ValidationRegex.IsMatch(value))
            throw new ArgumentException($"Value does not match required pattern", nameof(value));

        Value = value;
    }

    protected static Regex CreateDefaultRegex(int minLength, int maxLength) => new(
        $@"^[\p{{L}}\p{{M}}\p{{N}}]{{{minLength},{maxLength}}}$",
        RegexOptions.Singleline | RegexOptions.Compiled);
}