using BuildingBlocks.Domain.Results;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Features.Contracts.Commands.CancelContract;
using ContratacaoService.Application.Features.Contracts.Commands.CreateContract;
using ContratacaoService.Application.Features.Contracts.Queries.GetAllContracts;
using ContratacaoService.Application.Features.Contracts.Queries.GetContractById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContratacaoService.Api.Endpoints;

/// <summary>
/// Contract endpoints configuration using Minimal APIs.
/// </summary>
public static class ContractEndpoints
{
    public static void MapContractEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contracts")
            .WithTags("Contracts");

        group.MapPost("/", CreateContract)
            .WithName("CreateContract")
            .WithSummary("Create a contract from an approved proposal")
            .Produces<ContractResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllContracts)
            .WithName("GetAllContracts")
            .WithSummary("Get all contracts")
            .Produces<IReadOnlyList<ContractResponse>>();

        group.MapGet("/{id:guid}", GetContractById)
            .WithName("GetContractById")
            .WithSummary("Get a contract by ID")
            .Produces<ContractResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/cancel", CancelContract)
            .WithName("CancelContract")
            .WithSummary("Cancel a contract")
            .Produces<ContractResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateContract(
        [FromBody] CreateContractRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateContractCommand(request.ProposalId);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/contracts/{result.Value.Id}", result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> GetAllContracts(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllContractsQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> GetContractById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetContractByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> CancelContract(
        Guid id,
        [FromBody] CancelContractRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CancelContractCommand(id, request.Reason);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static IResult HandleFailure(Result result)
    {
        return result.Error.Code switch
        {
            "Contract.NotFound" or "Contract.ProposalNotFound" => Results.NotFound(
                CreateProblemDetails(
                    "Not Found",
                    result.Error.Description,
                    StatusCodes.Status404NotFound)),
            "Validation.Error" => Results.BadRequest(
                CreateProblemDetails(
                    "Validation Error",
                    result.Error.Description,
                    StatusCodes.Status400BadRequest)),
            "Contract.ExternalServiceUnavailable" => Results.Problem(
                CreateProblemDetails(
                    "Service Unavailable",
                    result.Error.Description,
                    StatusCodes.Status503ServiceUnavailable)),
            _ => Results.BadRequest(
                CreateProblemDetails(
                    "Bad Request",
                    result.Error.Description,
                    StatusCodes.Status400BadRequest))
        };
    }

    private static ProblemDetails CreateProblemDetails(string title, string detail, int statusCode)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}"
        };
    }
}

// Request DTOs
public sealed record CreateContractRequest(Guid ProposalId);

public sealed record CancelContractRequest(string Reason);
