using MediatR;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands;

public record ChargeCommand(Guid OrderId, decimal Amount) : IRequest<string>;

public class ChargeCommandHandler : IRequestHandler<ChargeCommand, string>
{
    private readonly IPaymentRepository _repository;

    public ChargeCommandHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(ChargeCommand request, CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken);

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

        return transaction.Id.ToString();
    }
}
