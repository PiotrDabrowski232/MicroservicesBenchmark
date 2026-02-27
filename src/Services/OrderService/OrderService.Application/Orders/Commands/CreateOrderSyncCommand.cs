using MediatR;

namespace OrderService.Application.Orders.Commands;

public record CreateOrderSyncCommand(Guid ProductId, int Quantity) : IRequest<Guid>;
