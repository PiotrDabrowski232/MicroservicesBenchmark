namespace SharedKernel
{
    public class ReserveProductMessage
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string CorrelationId { get; set; }
    }

    public class ProcessPaymentMessage
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CorrelationId { get; set; }
    }

    public class ProductReservedMessage
    {
        public string CorrelationId { get; set; }
        public bool Success { get; set; }
    }

    public class PaymentProcessedMessage
    {
        public string CorrelationId { get; set; }
        public bool Success { get; set; }
    }
}
