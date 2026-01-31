using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Domain.Errors;
using ContratacaoService.Domain.Events;
using FluentAssertions;
using Xunit;

namespace ContratacaoService.UnitTests.Domain;

public class ContractTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateContract()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var proposalNumber = "PROP-20240101-1234";

        // Act
        var result = Contract.Create(
            proposalId,
            proposalNumber,
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ProposalId.Should().Be(proposalId);
        result.Value.ProposalNumber.Should().Be(proposalNumber);
        result.Value.Status.Should().Be(ContractStatus.Active);
        result.Value.ContractNumber.Should().StartWith("CTR-");
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseContractCreatedEvent()
    {
        // Arrange
        var proposalId = Guid.NewGuid();

        // Act
        var result = Contract.Create(
            proposalId,
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)));

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<ContractCreatedEvent>();
    }

    [Fact]
    public void Cancel_WhenActive_ShouldCancelContract()
    {
        // Arrange
        var contract = Contract.Create(
            Guid.NewGuid(),
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1))).Value;

        const string reason = "Customer request";

        // Act
        var result = contract.Cancel(reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Cancelled);
        contract.CancelledAt.Should().NotBeNull();
        contract.CancellationReason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldReturnFailure()
    {
        // Arrange
        var contract = Contract.Create(
            Guid.NewGuid(),
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1))).Value;

        contract.Cancel("First cancellation");

        // Act
        var result = contract.Cancel("Second cancellation");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContractErrors.AlreadyCancelled);
    }

    [Fact]
    public void IsActive_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        var contract = Contract.Create(
            Guid.NewGuid(),
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1))).Value;

        // Act & Assert
        contract.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        var contract = Contract.Create(
            Guid.NewGuid(),
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1))).Value;

        contract.Cancel("Cancelled");

        // Act & Assert
        contract.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsCoverageValidOn_WhenDateInRange_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var endDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
        
        var contract = Contract.Create(
            Guid.NewGuid(),
            "PROP-20240101-1234",
            "12345678909",
            "João Silva",
            "Life",
            100000m,
            500m,
            "BRL",
            startDate,
            endDate).Value;

        var checkDate = startDate.AddMonths(6);

        // Act & Assert
        contract.IsCoverageValidOn(checkDate).Should().BeTrue();
    }
}
