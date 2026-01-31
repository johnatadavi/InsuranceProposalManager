using MediatR;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Application.Messaging;

/// <summary>
/// Handler for commands without response.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler for commands with response.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
