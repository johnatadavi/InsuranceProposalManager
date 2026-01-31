using MediatR;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Application.Messaging;

/// <summary>
/// Interface for commands that modify state.
/// Implements CQRS pattern - Commands for writes.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Interface for commands that return a value.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
