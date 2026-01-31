namespace BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for Aggregate Roots.
/// Aggregates are clusters of domain objects that can be treated as a single unit.
/// 
/// While this class currently has no additional behavior compared to Entity,
/// it serves as a marker to identify domain aggregates (per DDD pattern).
/// This makes it explicit which entities are aggregate roots and can:
/// - Be referenced by other aggregates via ID only
/// - Have repositories for persistence
/// - Enforce transactional boundaries
/// 
/// Future extensions may include:
/// - Version/concurrency control
/// - Aggregate-specific validation
/// - Event sourcing support
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
}
