using FluentAssertions;
using PropostaService.Domain.ValueObjects;
using Xunit;

namespace PropostaService.UnitTests.Domain;

public class CpfTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("123.456.789-09")]
    public void Create_WithValidCpf_ShouldCreateCpf(string cpf)
    {
        // Act
        var result = Cpf.Create(cpf);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("12345678909");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyCpf_ShouldThrowException(string? cpf)
    {
        // Act & Assert
        var act = () => Cpf.Create(cpf!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678901")] // Invalid check digits
    [InlineData("11111111111")] // All same digits
    [InlineData("12345")]       // Too short
    public void Create_WithInvalidCpf_ShouldThrowException(string cpf)
    {
        // Act & Assert
        var act = () => Cpf.Create(cpf);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToFormattedString_ShouldReturnFormattedCpf()
    {
        // Arrange
        var cpf = Cpf.Create("12345678909");

        // Act
        var formatted = cpf.ToFormattedString();

        // Assert
        formatted.Should().Be("123.456.789-09");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var cpf1 = Cpf.Create("12345678909");
        var cpf2 = Cpf.Create("123.456.789-09");

        // Assert
        cpf1.Should().Be(cpf2);
    }
}
