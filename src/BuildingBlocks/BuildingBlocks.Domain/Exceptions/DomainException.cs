namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Base exception for domain rule violations.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
