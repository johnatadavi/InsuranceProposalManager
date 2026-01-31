using BuildingBlocks.Application.Messaging;
using PropostaService.Application.DTOs;
using PropostaService.Domain.Enums;

namespace PropostaService.Application.Features.Proposals.Commands.CreateProposal;

/// <summary>
/// Command to create a new insurance proposal.
/// </summary>
public sealed record CreateProposalCommand(
    string HolderCpf,
    string HolderName,
    string HolderEmail,
    InsuranceType InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string? Description) : ICommand<ProposalResponse>;
