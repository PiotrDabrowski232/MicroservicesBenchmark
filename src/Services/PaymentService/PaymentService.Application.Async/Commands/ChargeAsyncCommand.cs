using MediatR;

using Messaging.Interfaces;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

using SharedKernel.Models;

namespace PaymentService.Application.Async.Commands
{
    public record ChargeAsyncCommand(Guid OrderId, decimal Amount, Guid CorrelactionId) : IRequest<string>;

    public class ChargeAsyncCommandHandler : IRequestHandler<ChargeAsyncCommand, string>
    {
        private readonly IPaymentRepository _repository;
        private readonly IMessageBus _messageBus;

        public ChargeAsyncCommandHandler(
            IMessageBus messageBus,
            IPaymentRepository repository)
        {
            _repository = repository;
            _messageBus = messageBus;
        }

        public async Task<string> Handle(ChargeAsyncCommand request, CancellationToken cancellationToken)
        {
            var isSuccess = Random.Shared.NextDouble() < 0.8; 

            var transaction = new TransactionRecord
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                Amount = request.Amount,
                Status = isSuccess ? "Success" : "Failed",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddTransactionAsync(transaction, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            if (isSuccess)
            {
                await _messageBus.PublishAsync(new PaymentProcessedMessage
                {
                    OrderId = request.OrderId,
                    CorrelationId = request.CorrelactionId
                }, cancellationToken);
            }
            else
            {
                await _messageBus.PublishAsync(new PaymentFailedMessage
                {
                    OrderId = request.OrderId,
                    CorrelationId = request.CorrelactionId,
                    Reason = "Payment authorization failed"
                }, cancellationToken);
            }

            return transaction.Id.ToString();
        }
    }
}
