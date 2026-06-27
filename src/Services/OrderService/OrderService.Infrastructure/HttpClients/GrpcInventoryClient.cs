using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Protos;

namespace OrderService.Infrastructure.HttpClients;

public class GrpcInventoryClient : IInventoryClient
{
    private readonly Inventory.InventoryClient _client;

    public GrpcInventoryClient(Inventory.InventoryClient client)
    {
        _client = client;
    }

    public async Task<bool> ReserveProductAsync(Guid productId, int quantity)
    {
        var request = new ReserveRequestMessage
        {
            ProductId = productId.ToString(),
            Quantity = quantity
        };

        var response = await _client.ReserveProductAsync(request);
        return response.Success;
    }

    public async Task<List<BenchmarkProductDto>> GetProductsBenchmarkAsync(int count)
    {
        var request = new GetProductsRequest { Count = count };
        var response = await _client.GetProductsBenchmarkAsync(request);

        var result = new List<BenchmarkProductDto>(response.Products.Count);

        foreach (var p in response.Products)
        {
            result.Add(new BenchmarkProductDto(p.Id, p.Name, p.Price, p.Description));
        }

        return result;
    }

    public async Task<TransportPingDto> GetTransportPingAsync()
    {
        var response = await _client.GetTransportPingAsync(new TransportPingRequest());

        return new TransportPingDto(response.Message, response.Value);
    }

    public async Task<List<BenchmarkComplexItemDto>> GetComplexPayloadBenchmarkAsync(int count)
    {
        var request = new GetComplexPayloadRequest { Count = count };
        var response = await _client.GetComplexPayloadBenchmarkAsync(request);

        var result = new List<BenchmarkComplexItemDto>(response.Items.Count);

        foreach (var p in response.Items)
        {
            var category = new ComplexItemCategoryDto(
                p.Category.Id, 
                p.Category.Name, 
                p.Category.ParentCategory != null ? new ComplexItemCategoryDto(p.Category.ParentCategory.Id, p.Category.ParentCategory.Name, p.Category.ParentCategory.ParentCategory != null ? new ComplexItemCategoryDto(p.Category.ParentCategory.ParentCategory.Id, p.Category.ParentCategory.ParentCategory.Name, null) : null) : null
            );

            var translations = p.Translations.Select(t => new ComplexItemTranslationDto(t.LanguageCode, t.Title, t.Description)).ToList();
            var metadata = new ComplexItemMetadataDto(p.Metadata.Manufacturer, p.Metadata.Weight, p.Metadata.IsAvailable, p.Metadata.Tags);
            var reviews = p.Reviews.Select(r => new ComplexItemReviewDto(r.Reviewer, r.Rating, r.Comment, r.Date)).ToList();

            result.Add(new BenchmarkComplexItemDto(p.Id, p.Name, p.Price, category, translations, metadata, reviews));
        }

        return result;
    }
}
