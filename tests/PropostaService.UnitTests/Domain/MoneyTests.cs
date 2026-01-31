using FluentAssertions;
using PropostaService.Domain.ValueObjects;
using Xunit;

namespace PropostaService.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        // Act
        var money = Money.Create(100.50m, "BRL");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Money.Create(-100m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidCurrency_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Money.Create(100m, "INVALID");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "BRL");
        var money2 = Money.Create(50m, "USD");

        // Act & Assert
        var act = () => money1.Add(money2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(50m);
    }

    [Fact]
    public void Subtract_ResultingNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(50m);
        var money2 = Money.Create(100m);

        // Act & Assert
        var act = () => money1.Subtract(money2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MultiplyBy_ShouldMultiplyAmount()
    {
        // Arrange
        var money = Money.Create(100m);

        // Act
        var result = money.MultiplyBy(1.5m);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100m, "BRL");
        var money2 = Money.Create(100m, "BRL");

        // Assert
        money1.Should().Be(money2);
    }
}
