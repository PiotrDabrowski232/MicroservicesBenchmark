using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Interfaces
{
    public interface IOrderWorkflowOrchestrator
    {
        Task<Guid> ProcessAsync(CreateOrderSyncCommand command, CancellationToken cancellationToken);
    }
}
