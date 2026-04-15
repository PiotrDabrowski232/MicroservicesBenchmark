namespace SharedKernel.Options
{
    public class RabbitMqOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string ResponseExchange { get; set; } = string.Empty;
    }
}
