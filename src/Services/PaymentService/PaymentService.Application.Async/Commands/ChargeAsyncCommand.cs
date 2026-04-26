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
            var transaction = new TransactionRecord
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                Amount = request.Amount,
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddTransactionAsync(transaction, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _messageBus.PublishAsync<PaymentProcessedMessage>(new PaymentProcessedMessage { 
                OrderId = request.OrderId, 
                CorrelationId = request.CorrelactionId 
            }, cancellationToken);  

            return transaction.Id.ToString();
        }
    }
}
