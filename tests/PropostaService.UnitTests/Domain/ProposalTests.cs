using FluentAssertions;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Errors;
using PropostaService.Domain.Events;
using PropostaService.UnitTests.Builders;
using Xunit;

namespace PropostaService.UnitTests.Domain;

public class ProposalTests
{
    private readonly ProposalDataBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldCreateProposal()
    {
        // Arrange
        var data = _builder.Build();

        // Act
        var result = Proposal.Create(
            data.HolderCpf,
            data.HolderName,
            data.HolderEmail,
            data.InsuranceType,
            data.CoverageAmount,
            data.PremiumAmount,
            data.StartDate,
            data.EndDate,
            data.Description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.HolderName.Should().Be(data.HolderName);
        result.Value.InsuranceType.Should().Be(data.InsuranceType);
        result.Value.Status.Should().Be(ProposalStatus.UnderAnalysis);
        result.Value.ProposalNumber.Should().StartWith("PROP-");
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseProposalCreatedEvent()
    {
        // Arrange
        var data = _builder.Build();

        // Act
        var result = Proposal.Create(
            data.HolderCpf,
            data.HolderName,
            data.HolderEmail,
            data.InsuranceType,
            data.CoverageAmount,
            data.PremiumAmount,
            data.StartDate,
            data.EndDate,
            data.Description);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<ProposalCreatedEvent>();
    }

    [Fact]
    public void Create_WithInvalidCoverageAmount_ShouldReturnFailure()
    {
        // Arrange
        var data = _builder.WithCoverageAmount(0).Build();

        // Act
        var result = Proposal.Create(
            data.HolderCpf,
            data.HolderName,
            data.HolderEmail,
            data.InsuranceType,
            data.CoverageAmount,
            data.PremiumAmount,
            data.StartDate,
            data.EndDate,
            data.Description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProposalErrors.InvalidCoverageAmount);
    }

    [Fact]
    public void Create_WithInvalidPremiumAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var data = _builder.WithPremiumAmount(-100).Build();

        // Act & Assert - Money.Create throws for negative values
        var act = () => Proposal.Create(
            data.HolderCpf,
            data.HolderName,
            data.HolderEmail,
            data.InsuranceType,
            data.CoverageAmount,
            data.PremiumAmount,
            data.StartDate,
            data.EndDate,
            data.Description);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Approve_WhenUnderAnalysis_ShouldApproveProposal()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.ClearDomainEvents();

        // Act
        var result = proposal.Approve();

        // Assert
        result.IsSuccess.Should().BeTrue();
        proposal.Status.Should().Be(ProposalStatus.Approved);
        proposal.ApprovedAt.Should().NotBeNull();
        proposal.DomainEvents.Should().HaveCount(2); // StatusChanged + Approved
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldReturnFailure()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.Approve();

        // Act
        var result = proposal.Approve();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProposalErrors.AlreadyApproved);
    }

    [Fact]
    public void Reject_WhenUnderAnalysis_ShouldRejectProposal()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.ClearDomainEvents();
        const string reason = "Risk assessment failed";

        // Act
        var result = proposal.Reject(reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        proposal.Status.Should().Be(ProposalStatus.Rejected);
        proposal.RejectedAt.Should().NotBeNull();
        proposal.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_ShouldReturnFailure()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.Approve();

        // Act
        var result = proposal.Reject("Some reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProposalErrors.AlreadyApproved);
    }

    [Fact]
    public void MarkAsContracted_WhenApproved_ShouldMarkAsContracted()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.Approve();
        proposal.ClearDomainEvents();

        // Act
        var result = proposal.MarkAsContracted();

        // Assert
        result.IsSuccess.Should().BeTrue();
        proposal.Status.Should().Be(ProposalStatus.Contracted);
        proposal.ContractedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsContracted_WhenNotApproved_ShouldReturnFailure()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        // Act
        var result = proposal.MarkAsContracted();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProposalErrors.NotApproved);
    }

    [Fact]
    public void CanBeContracted_WhenApproved_ShouldReturnTrue()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        proposal.Approve();

        // Act & Assert
        proposal.CanBeContracted().Should().BeTrue();
    }

    [Fact]
    public void CanBeContracted_WhenNotApproved_ShouldReturnFalse()
    {
        // Arrange
        var data = _builder.Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        // Act & Assert
        proposal.CanBeContracted().Should().BeFalse();
    }
}
