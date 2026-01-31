using BuildingBlocks.Domain.Primitives;
using BuildingBlocks.Domain.Results;
using ContratacaoService.Domain.Enums;
using ContratacaoService.Domain.Events;

namespace ContratacaoService.Domain.Entities;

/// <summary>
/// Aggregate Root representing an Insurance Contract.
/// A contract is created when an approved proposal is contracted.
/// </summary>
public sealed class Contract : AggregateRoot
{
    private Contract(
        Guid id,
        string contractNumber,
        Guid proposalId,
        string proposalNumber,
        string holderCpf,
        string holderName,
        string insuranceType,
        decimal coverageAmount,
        decimal premiumAmount,
        string currency,
        DateOnly coverageStartDate,
        DateOnly coverageEndDate)
        : base(id)
    {
        ContractNumber = contractNumber;
        ProposalId = proposalId;
        ProposalNumber = proposalNumber;
        HolderCpf = holderCpf;
        HolderName = holderName;
        InsuranceType = insuranceType;
        CoverageAmount = coverageAmount;
        PremiumAmount = premiumAmount;
        Currency = currency;
        CoverageStartDate = coverageStartDate;
        CoverageEndDate = coverageEndDate;
        Status = ContractStatus.Active;
        ContractedAt = DateTime.UtcNow;
    }

    // Private parameterless constructor for EF Core
#pragma warning disable CS8618
    private Contract() : base() { }
#pragma warning restore CS8618

    public string ContractNumber { get; private set; }
    public Guid ProposalId { get; private set; }
    public string ProposalNumber { get; private set; }
    public string HolderCpf { get; private set; }
    public string HolderName { get; private set; }
    public string InsuranceType { get; private set; }
    public decimal CoverageAmount { get; private set; }
    public decimal PremiumAmount { get; private set; }
    public string Currency { get; private set; }
    public DateOnly CoverageStartDate { get; private set; }
    public DateOnly CoverageEndDate { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTime ContractedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Factory method to create a new Contract from an approved proposal.
    /// </summary>
    public static Result<Contract> Create(
        Guid proposalId,
        string proposalNumber,
        string holderCpf,
        string holderName,
        string insuranceType,
        decimal coverageAmount,
        decimal premiumAmount,
        string currency,
        DateOnly coverageStartDate,
        DateOnly coverageEndDate)
    {
        var contractNumber = GenerateContractNumber();

        var contract = new Contract(
            Guid.NewGuid(),
            contractNumber,
            proposalId,
            proposalNumber,
            holderCpf,
            holderName,
            insuranceType,
            coverageAmount,
            premiumAmount,
            currency,
            coverageStartDate,
            coverageEndDate);

        contract.RaiseDomainEvent(new ContractCreatedEvent(
            contract.Id,
            contract.ProposalId,
            contract.ContractNumber,
            contract.ContractedAt));

        return Result.Success(contract);
    }

    /// <summary>
    /// Cancels the contract.
    /// </summary>
    public Result Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        if (Status == ContractStatus.Cancelled)
            return Result.Failure(Errors.ContractErrors.AlreadyCancelled);

        if (Status == ContractStatus.Expired)
            return Result.Failure(Errors.ContractErrors.AlreadyExpired);

        Status = ContractStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason.Trim();

        return Result.Success();
    }

    /// <summary>
    /// Checks if the contract is currently active.
    /// </summary>
    public bool IsActive() => Status == ContractStatus.Active;

    /// <summary>
    /// Checks if coverage is valid for a given date.
    /// </summary>
    public bool IsCoverageValidOn(DateOnly date)
    {
        return Status == ContractStatus.Active &&
               date >= CoverageStartDate &&
               date <= CoverageEndDate;
    }

    private static string GenerateContractNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"CTR-{timestamp}-{random}";
    }
}
