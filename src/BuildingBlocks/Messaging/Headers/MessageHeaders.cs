namespace BuildingBlocks.Messaging.Headers;

public static class MessageHeaders
{
    public const string EventType = "event-type";
    public const string EventVersion = "event-version";
    public const string MessageId = "message-id";
    public const string CorrelationId = "correlation-id";
    public const string CausationId = "causation-id";
    public const string TenantId = "tenant-id";
    public const string UserId = "user-id";
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
}