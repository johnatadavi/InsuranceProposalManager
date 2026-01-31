using BuildingBlocks.Domain.Primitives;
using System.Text.RegularExpressions;

namespace PropostaService.Domain.ValueObjects;

/// <summary>
/// Value Object representing a Brazilian CPF (individual taxpayer registry).
/// Validates format and check digits.
/// </summary>
public sealed partial class Cpf : ValueObject
{
    private static readonly Regex CpfRegex = GenerateCpfRegex();

    private Cpf(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Cpf Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF cannot be empty", nameof(cpf));

        var cleanCpf = CleanCpf(cpf);

        if (!IsValidCpf(cleanCpf))
            throw new ArgumentException("Invalid CPF", nameof(cpf));

        return new Cpf(cleanCpf);
    }

    private static string CleanCpf(string cpf)
    {
        return CpfRegex.Replace(cpf, string.Empty);
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        // Check if all digits are the same (invalid CPF)
        if (cpf.Distinct().Count() == 1)
            return false;

        // Validate check digits
        var numbers = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        // First check digit
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        var remainder = sum % 11;
        var firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        if (numbers[9] != firstCheckDigit)
            return false;

        // Second check digit
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);

        remainder = sum % 11;
        var secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        return numbers[10] == secondCheckDigit;
    }

    public string ToFormattedString()
    {
        return $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex GenerateCpfRegex();
}
