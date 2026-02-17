namespace BuildingBlocks.Messaging.Contracts;

public sealed record MessageEnvelope(
    IReadOnlyDictionary<string, string> Headers,
    string Body
);