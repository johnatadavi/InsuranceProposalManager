using BuildingBlocks.Domain.Primitives;
using System.Text.RegularExpressions;

namespace PropostaService.Domain.ValueObjects;

/// <summary>
/// Value Object representing an email address.
/// Validates email format.
/// </summary>
public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = GenerateEmailRegex();

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 256)
            throw new ArgumentException("Email is too long", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex GenerateEmailRegex();
}
