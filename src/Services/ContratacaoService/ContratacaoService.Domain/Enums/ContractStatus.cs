namespace ContratacaoService.Domain.Enums;

/// <summary>
/// Represents the status of a contract.
/// </summary>
public enum ContractStatus
{
    /// <summary>
    /// Contract is active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Contract has been cancelled
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Contract has expired
    /// </summary>
    Expired = 3
}
