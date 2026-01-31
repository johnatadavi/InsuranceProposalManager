using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Abstractions;
using PropostaService.Application.Features.Proposals.Commands.ApproveProposal;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using PropostaService.UnitTests.Builders;
using Xunit;

namespace PropostaService.UnitTests.Application;

public class ApproveProposalCommandHandlerTests
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApproveProposalCommandHandler _handler;

    public ApproveProposalCommandHandlerTests()
    {
        _proposalRepository = Substitute.For<IProposalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ApproveProposalCommandHandler(_proposalRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithExistingProposal_ShouldApprove()
    {
        // Arrange
        var data = new ProposalDataBuilder().Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;

        var command = new ApproveProposalCommand(proposal.Id);

        _proposalRepository.GetByIdAsync(proposal.Id, Arg.Any<CancellationToken>())
            .Returns(proposal);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Approved");
        _proposalRepository.Received(1).Update(proposal);
    }

    [Fact]
    public async Task Handle_WithNonExistingProposal_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ApproveProposalCommand(Guid.NewGuid());

        _proposalRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Proposal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Proposal.NotFound");
    }

    [Fact]
    public async Task Handle_WithAlreadyApprovedProposal_ShouldReturnError()
    {
        // Arrange
        var data = new ProposalDataBuilder().Build();
        var proposal = Proposal.Create(
            data.HolderCpf, data.HolderName, data.HolderEmail,
            data.InsuranceType, data.CoverageAmount, data.PremiumAmount,
            data.StartDate, data.EndDate, data.Description).Value;
        proposal.Approve();

        var command = new ApproveProposalCommand(proposal.Id);

        _proposalRepository.GetByIdAsync(proposal.Id, Arg.Any<CancellationToken>())
            .Returns(proposal);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Proposal.AlreadyApproved");
    }
}
