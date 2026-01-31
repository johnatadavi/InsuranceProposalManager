using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.Abstractions;
using ContratacaoService.Application.Features.Contracts.Commands.CreateContract;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ContratacaoService.UnitTests.Application;

public class CreateContractCommandHandlerTests
{
    private readonly IContractRepository _contractRepository;
    private readonly IProposalService _proposalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateContractCommandHandler _handler;

    public CreateContractCommandHandlerTests()
    {
        _contractRepository = Substitute.For<IContractRepository>();
        _proposalService = Substitute.For<IProposalService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateContractCommandHandler(_contractRepository, _proposalService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithApprovedProposal_ShouldCreateContract()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var command = new CreateContractCommand(proposalId);

        var proposalDto = new ProposalDto(
            proposalId,
            "PROP-20240101-1234",
            "123.456.789-09",
            "João Silva",
            "joao@email.com",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            "Approved",
            CanBeContracted: true);

        _contractRepository.ExistsByProposalIdAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(false);
        _proposalService.GetProposalByIdAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(proposalDto));
        _proposalService.MarkProposalAsContractedAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ProposalId.Should().Be(proposalId);
        result.Value.Status.Should().Be("Active");

        await _contractRepository.Received(1).AddAsync(
            Arg.Any<Contract>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithProposalAlreadyContracted_ShouldReturnFailure()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var command = new CreateContractCommand(proposalId);

        _contractRepository.ExistsByProposalIdAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Contract.ProposalAlreadyContracted");

        await _proposalService.DidNotReceive().GetProposalByIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNotApprovedProposal_ShouldReturnFailure()
    {
        // Arrange
        var proposalId = Guid.NewGuid();
        var command = new CreateContractCommand(proposalId);

        var proposalDto = new ProposalDto(
            proposalId,
            "PROP-20240101-1234",
            "123.456.789-09",
            "João Silva",
            "joao@email.com",
            "Life",
            100000m,
            500m,
            "BRL",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            "UnderAnalysis",
            CanBeContracted: false);

        _contractRepository.ExistsByProposalIdAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(false);
        _proposalService.GetProposalByIdAsync(proposalId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(proposalDto));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Contract.ProposalNotApproved");

        await _contractRepository.DidNotReceive().AddAsync(
            Arg.Any<Contract>(),
            Arg.Any<CancellationToken>());
    }
}
