namespace InventoryService.Application.Async.Dto
{
    public record OrderDto(Guid OrderId, Guid CorrelationId);
    
}
