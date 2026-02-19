using BuildingBlocks.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.DTOs;
using PropostaService.Application.Features.Proposals.Commands.ApproveProposal;
using PropostaService.Application.Features.Proposals.Commands.CreateProposal;
using PropostaService.Application.Features.Proposals.Commands.MarkAsContracted;
using PropostaService.Application.Features.Proposals.Commands.RejectProposal;
using PropostaService.Application.Features.Proposals.Queries.GetAllProposals;
using PropostaService.Application.Features.Proposals.Queries.GetProposalById;
using PropostaService.Domain.Enums;

namespace PropostaService.Api.Endpoints;

/// <summary>
/// Proposal endpoints configuration using Minimal APIs.
/// </summary>
public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proposals")
            .WithTags("Proposals");

        group.MapPost("/", CreateProposal)
            .WithName("CreateProposal")
            .WithSummary("Create a new insurance proposal")
            .Produces<ProposalResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAllProposals)
            .WithName("GetAllProposals")
            .WithSummary("Get all proposals")
            .Produces<IReadOnlyList<ProposalResponse>>();

        group.MapGet("/{id:guid}", GetProposalById)
            .WithName("GetProposalById")
            .WithSummary("Get a proposal by ID")
            .Produces<ProposalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/approve", ApproveProposal)
            .WithName("ApproveProposal")
            .WithSummary("Approve a proposal")
            .Produces<ProposalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/reject", RejectProposal)
            .WithName("RejectProposal")
            .WithSummary("Reject a proposal")
            .Produces<ProposalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/mark-contracted", MarkAsContracted)
            .WithName("MarkAsContracted")
            .WithSummary("Mark a proposal as contracted (internal use)")
            .Produces<ProposalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateProposal(
        [FromBody] CreateProposalRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProposalCommand(
            request.HolderCpf,
            request.HolderName,
            request.HolderEmail,
            request.InsuranceType,
            request.CoverageAmount,
            request.PremiumAmount,
            request.CoverageStartDate,
            request.CoverageEndDate,
            request.Description);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/proposals/{result.Value.Id}", result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> GetAllProposals(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllProposalsQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> GetProposalById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProposalByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> ApproveProposal(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ApproveProposalCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> RejectProposal(
        Guid id,
        [FromBody] RejectProposalRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RejectProposalCommand(id, request.Reason);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static async Task<IResult> MarkAsContracted(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new MarkAsContractedCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleFailure(result);
    }

    private static IResult HandleFailure(Result result)
    {
        return result.Error.Code switch
        {
            "Proposal.NotFound" => Results.NotFound(CreateProblemDetails(
                "Not Found",
                result.Error.Description,
                StatusCodes.Status404NotFound)),
            "Validation.Error" => Results.BadRequest(CreateProblemDetails(
                "Validation Error",
                result.Error.Description,
                StatusCodes.Status400BadRequest)),
            _ => Results.BadRequest(CreateProblemDetails(
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
public sealed record CreateProposalRequest(
    string HolderCpf,
    string HolderName,
    string HolderEmail,
    InsuranceType InsuranceType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    string? Description);

public sealed record RejectProposalRequest(string Reason);
