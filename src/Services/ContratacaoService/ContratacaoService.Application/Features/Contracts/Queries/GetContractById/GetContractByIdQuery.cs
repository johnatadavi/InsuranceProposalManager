using BuildingBlocks.Application.Messaging;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Features.Contracts.Queries.GetContractById;

/// <summary>
/// Query to get a contract by its ID.
/// </summary>
public sealed record GetContractByIdQuery(Guid ContractId) : IQuery<ContractResponse>;
