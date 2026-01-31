using MediatR;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Application.Messaging;

/// <summary>
/// Handler for queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
