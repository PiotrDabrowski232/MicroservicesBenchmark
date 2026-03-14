using MediatR;
using InventoryService.Application.Interfaces;

namespace InventoryService.Application.Commands;

public record ReserveProductCommand(Guid ProductId, int Quantity) : IRequest<bool>;

public class ReserveProductCommandHandler : IRequestHandler<ReserveProductCommand, bool>
{
    private readonly IInventoryRepository _repository;

    public ReserveProductCommandHandler(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ReserveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product == null || product.StockQuantity < request.Quantity)
        {
            return false;
        }

        product.StockQuantity -= request.Quantity;
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
