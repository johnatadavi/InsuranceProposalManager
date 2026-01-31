using BuildingBlocks.Domain.Primitives;
using BuildingBlocks.Domain.Results;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Errors;
using PropostaService.Domain.Events;
using PropostaService.Domain.ValueObjects;

namespace PropostaService.Domain.Entities;

/// <summary>
/// Aggregate Root representing an Insurance Proposal.
/// Contains all business rules and invariants for proposal lifecycle.
/// Rich Domain Model - No anemic getters/setters.
/// </summary>
public sealed class Proposal : AggregateRoot
{
    private Proposal(
        Guid id,
        string proposalNumber,
        Cpf holderCpf,
        string holderName,
        Email holderEmail,
        InsuranceType insuranceType,
        Money coverageAmount,
        Money premiumAmount,
        CoveragePeriod coveragePeriod,
        string? description)
        : base(id)
    {
        ProposalNumber = proposalNumber;
        HolderCpf = holderCpf;
        HolderName = holderName;
        HolderEmail = holderEmail;
        InsuranceType = insuranceType;
        CoverageAmount = coverageAmount;
        PremiumAmount = premiumAmount;
        CoveragePeriod = coveragePeriod;
        Description = description;
        Status = ProposalStatus.UnderAnalysis;
        CreatedAt = DateTime.UtcNow;
    }

    // Private parameterless constructor for EF Core
#pragma warning disable CS8618
    private Proposal() : base() { }
#pragma warning restore CS8618

    public string ProposalNumber { get; private set; }
    public Cpf HolderCpf { get; private set; }
    public string HolderName { get; private set; }
    public Email HolderEmail { get; private set; }
    public InsuranceType InsuranceType { get; private set; }
    public Money CoverageAmount { get; private set; }
    public Money PremiumAmount { get; private set; }
    public CoveragePeriod CoveragePeriod { get; private set; }
    public string? Description { get; private set; }
    public ProposalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime? ContractedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Factory method to create a new Proposal.
    /// All validations are performed here.
    /// </summary>
    public static Result<Proposal> Create(
        string holderCpf,
        string holderName,
        string holderEmail,
        InsuranceType insuranceType,
        decimal coverageAmount,
        decimal premiumAmount,
        DateOnly startDate,
        DateOnly endDate,
        string? description = null)
    {
        // Validate and create Value Objects
        var cpf = Cpf.Create(holderCpf);
        var email = Email.Create(holderEmail);
        var coverage = Money.Create(coverageAmount);
        var premium = Money.Create(premiumAmount);
        var period = CoveragePeriod.Create(startDate, endDate);

        if (coverageAmount <= 0)
            return Result.Failure<Proposal>(ProposalErrors.InvalidCoverageAmount);

        if (premiumAmount <= 0)
            return Result.Failure<Proposal>(ProposalErrors.InvalidPremiumAmount);

        var proposalNumber = GenerateProposalNumber();

        var proposal = new Proposal(
            Guid.NewGuid(),
            proposalNumber,
            cpf,
            holderName.Trim(),
            email,
            insuranceType,
            coverage,
            premium,
            period,
            description?.Trim());

        proposal.RaiseDomainEvent(new ProposalCreatedEvent(
            proposal.Id,
            proposal.HolderCpf.Value,
            proposal.InsuranceType.ToString(),
            proposal.CoverageAmount.Amount,
            proposal.PremiumAmount.Amount));

        return Result.Success(proposal);
    }

    /// <summary>
    /// Approves the proposal if it's under analysis.
    /// </summary>
    public Result Approve()
    {
        if (Status == ProposalStatus.Approved)
            return Result.Failure(ProposalErrors.AlreadyApproved);

        if (Status == ProposalStatus.Rejected)
            return Result.Failure(ProposalErrors.AlreadyRejected);

        if (Status == ProposalStatus.Contracted)
            return Result.Failure(ProposalErrors.AlreadyContracted);

        var oldStatus = Status;
        Status = ProposalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProposalStatusChangedEvent(
            Id,
            oldStatus.ToString(),
            Status.ToString()));

        RaiseDomainEvent(new ProposalApprovedEvent(
            Id,
            HolderCpf.Value,
            InsuranceType.ToString(),
            CoverageAmount.Amount,
            PremiumAmount.Amount));

        return Result.Success();
    }

    /// <summary>
    /// Rejects the proposal if it's under analysis.
    /// </summary>
    public Result Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        if (Status == ProposalStatus.Approved)
            return Result.Failure(ProposalErrors.AlreadyApproved);

        if (Status == ProposalStatus.Rejected)
            return Result.Failure(ProposalErrors.AlreadyRejected);

        if (Status == ProposalStatus.Contracted)
            return Result.Failure(ProposalErrors.AlreadyContracted);

        var oldStatus = Status;
        Status = ProposalStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProposalStatusChangedEvent(
            Id,
            oldStatus.ToString(),
            Status.ToString()));

        return Result.Success();
    }

    /// <summary>
    /// Marks the proposal as contracted. Only approved proposals can be contracted.
    /// This is called by the Contratação service via the domain event or direct call.
    /// </summary>
    public Result MarkAsContracted()
    {
        if (Status != ProposalStatus.Approved)
            return Result.Failure(ProposalErrors.NotApproved);

        var oldStatus = Status;
        Status = ProposalStatus.Contracted;
        ContractedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProposalStatusChangedEvent(
            Id,
            oldStatus.ToString(),
            Status.ToString()));

        return Result.Success();
    }

    /// <summary>
    /// Checks if the proposal can be contracted.
    /// </summary>
    public bool CanBeContracted() => Status == ProposalStatus.Approved;

    /// <summary>
    /// Updates proposal details (only allowed when under analysis).
    /// </summary>
    public Result UpdateDetails(
        string holderName,
        string holderEmail,
        decimal coverageAmount,
        decimal premiumAmount,
        string? description)
    {
        if (Status != ProposalStatus.UnderAnalysis)
            return Result.Failure(ProposalErrors.InvalidStatusTransition);

        HolderName = holderName.Trim();
        HolderEmail = Email.Create(holderEmail);
        CoverageAmount = Money.Create(coverageAmount);
        PremiumAmount = Money.Create(premiumAmount);
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private static string GenerateProposalNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"PROP-{timestamp}-{random}";
    }
}
