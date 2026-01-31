namespace BuildingBlocks.Domain.Results;

/// <summary>
/// Represents a domain error with code and description.
/// Immutable record for error handling without exceptions.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public static implicit operator string(Error error) => error.Code;
}
