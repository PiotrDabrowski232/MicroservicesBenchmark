using OrderService.Application.Interfaces;
using OrderService.Application.Orders.Commands;

namespace OrderService.Infrastructure.Orchestrators
{
    public class AsyncOrderWorkflowOrchestrator : IOrderWorkflowOrchestrator
    {
        public Task<Guid> ProcessAsync(CreateOrderSyncCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
