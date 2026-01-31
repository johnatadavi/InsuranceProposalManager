using BuildingBlocks.Domain.Primitives;

namespace PropostaService.Domain.ValueObjects;

/// <summary>
/// Value Object representing a date range for coverage period.
/// </summary>
public sealed class CoveragePeriod : ValueObject
{
    private CoveragePeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public int DurationInDays => EndDate.DayNumber - StartDate.DayNumber;
    public int DurationInMonths => ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;

    public static CoveragePeriod Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        if (startDate < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Start date cannot be in the past");

        return new CoveragePeriod(startDate, endDate);
    }

    public bool Contains(DateOnly date)
    {
        return date >= StartDate && date <= EndDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
