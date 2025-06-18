using System.Text.RegularExpressions;

namespace TelemetryManager.Core.Data.ValueObjects;

public sealed record Name
{
    public const int MIN_LENGTH = 5;
    public const int MAX_LENGTH = 30;

    private static readonly Regex ValidationRegex = new(
        $@"^[\p{{L}}\p{{M}}\p{{N}}]{{{MIN_LENGTH},{MAX_LENGTH}}}$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    public string Value { get; }

    public Name(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Name is not valid", nameof(value));

        Value = value;
    }

    public static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length >= MIN_LENGTH &&
        value.Length <= MAX_LENGTH &&
        ValidationRegex.IsMatch(value);

    public override string ToString() => Value;
}