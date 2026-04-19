using SharedKernel.Enums;

namespace OrderService.Application.DTOs
{
    public record OrderDto(Guid Id, string Status);
}
