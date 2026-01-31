using FluentAssertions;
using NSubstitute;
using PropostaService.Application.Abstractions;
using PropostaService.Application.Features.Proposals.Commands.CreateProposal;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Repositories;
using Xunit;

namespace PropostaService.UnitTests.Application;

public class CreateProposalCommandHandlerTests
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateProposalCommandHandler _handler;

    public CreateProposalCommandHandlerTests()
    {
        _proposalRepository = Substitute.For<IProposalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateProposalCommandHandler(_proposalRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProposal()
    {
        // Arrange
        var command = new CreateProposalCommand(
            "12345678909",
            "João Silva",
            "joao@email.com",
            InsuranceType.Life,
            100000m,
            500m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            "Test proposal");

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.HolderName.Should().Be("João Silva");
        result.Value.InsuranceType.Should().Be("Life");
        result.Value.Status.Should().Be("UnderAnalysis");

        await _proposalRepository.Received(1).AddAsync(
            Arg.Any<Proposal>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCoverageAmount_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateProposalCommand(
            "12345678909",
            "João Silva",
            "joao@email.com",
            InsuranceType.Life,
            0, // Invalid
            500m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Proposal.InvalidCoverageAmount");

        await _proposalRepository.DidNotReceive().AddAsync(
            Arg.Any<Proposal>(),
            Arg.Any<CancellationToken>());
    }
}
