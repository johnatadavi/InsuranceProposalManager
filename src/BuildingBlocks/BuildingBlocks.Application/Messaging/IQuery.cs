using MediatR;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Application.Messaging;

/// <summary>
/// Interface for queries that read data.
/// Implements CQRS pattern - Queries for reads.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
