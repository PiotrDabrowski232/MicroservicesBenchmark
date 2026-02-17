using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions;

namespace BuildingBlocks.Messaging.Serialization;

public sealed class MessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T message)
        => JsonSerializer.Serialize(message, Options);

    public T Deserialize<T>(string serializedMessage)
    {
        var value = JsonSerializer.Deserialize<T>(serializedMessage, Options);
        return value ?? throw new InvalidOperationException(
            $"Cannot deserialize payload to {typeof(T).Name}.");
    }
}