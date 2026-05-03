using Confluent.Kafka;
using Confluent.Kafka.Admin;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SharedKernel.Options;

namespace Messaging.HostedServices
{
    public class KafkaTopicInitializerHostedService : IHostedService
    {
        private readonly CommunicationOptions _options;
        private readonly Dictionary<string, string> _connections;
        private readonly ILogger<KafkaTopicInitializerHostedService> _logger;

        public KafkaTopicInitializerHostedService(
            CommunicationOptions options,
            Dictionary<string, string> connections,
            ILogger<KafkaTopicInitializerHostedService> logger)
        {
            _options = options;
            _connections = connections;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.AsyncProvider.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
                return;

            var connection = _connections["Kafka"];

            using var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = connection
            }).Build();

            var topics = _options.Messaging.Routes
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

            try
            {
                await adminClient.CreateTopicsAsync(topics);

                _logger.LogInformation("Kafka topics initialized.");
            }
            catch (CreateTopicsException ex)
            {
                var errors = ex.Results
                    .Where(x =>
                        x.Error.Code != ErrorCode.NoError &&
                        x.Error.Code != ErrorCode.TopicAlreadyExists)
                    .ToList();

                if (errors.Any())
                {
                    throw new InvalidOperationException(
                        "Kafka topic initialization failed: " +
                        string.Join(", ", errors.Select(x => $"{x.Topic}: {x.Error.Reason}")));
                }

                _logger.LogInformation("Kafka topics already exist.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
