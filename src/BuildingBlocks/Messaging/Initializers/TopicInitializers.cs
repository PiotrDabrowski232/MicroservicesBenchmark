using Confluent.Kafka;
using Confluent.Kafka.Admin;

using SharedKernel.Options;

namespace Messaging.Initializers
{
    public class TopicInitializers
    {
        private readonly string _brokerConnectionString;
        private readonly List<MessageRouteOptions> _routes;

        public TopicInitializers(
            List<MessageRouteOptions> routes,
            string brokerConnectionString)
        {
            _routes = routes;
            _brokerConnectionString = brokerConnectionString;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = _brokerConnectionString
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var topics = _routes
                .Where(x => !string.IsNullOrWhiteSpace(x.Topic))
                .Select(x => x.Topic!)
                .Distinct()
                .Select(topic => new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                })
                .ToList();

            if (!topics.Any())
                return;

            try
            {
                await adminClient.CreateTopicsAsync(topics);
            }
            catch (CreateTopicsException ex)
            {
                var realErrors = ex.Results
                    .Where(x =>
                        x.Error.Code != ErrorCode.NoError &&
                        x.Error.Code != ErrorCode.TopicAlreadyExists)
                    .ToList();

                if (realErrors.Any())
                {
                    var errors = string.Join(", ", realErrors.Select(x =>
                        $"{x.Topic}: {x.Error.Reason}"));

                    throw new InvalidOperationException($"Kafka topic creation failed: {errors}");
                }
            }
        }
    }
}
