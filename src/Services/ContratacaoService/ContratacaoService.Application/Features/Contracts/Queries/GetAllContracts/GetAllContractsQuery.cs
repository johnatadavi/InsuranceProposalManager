using BuildingBlocks.Application.Messaging;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Features.Contracts.Queries.GetAllContracts;

/// <summary>
/// Query to get all contracts.
/// </summary>
public sealed record GetAllContractsQuery : IQuery<IReadOnlyList<ContractResponse>>;
