using MediatR;

using OrderService.Application.Interfaces;

namespace OrderService.Application.Orders.Commands;

public record CreateOrderSyncCommand(Guid ProductId, int Quantity) : IRequest<Guid>;

public class CreateOrderSyncCommandHandler : IRequestHandler<CreateOrderSyncCommand, Guid>
{
    private readonly IOrderWorkflowOrchestrator _workflowOrchestrator;
    public CreateOrderSyncCommandHandler(IOrderWorkflowOrchestrator workflowOrchestrator)
    {
        _workflowOrchestrator = workflowOrchestrator;
    }

    public async Task<Guid> Handle(CreateOrderSyncCommand request, CancellationToken cancellationToken)
    {
        return await _workflowOrchestrator.ProcessAsync(request, cancellationToken);
    }
}
