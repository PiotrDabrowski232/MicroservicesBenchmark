namespace BuildingBlocks.Messaging.Contracts;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = default!;
    public string? CausationId { get; init; }
    public string? TenantId { get; init; }
    public int Version { get; init; } = 1;
}