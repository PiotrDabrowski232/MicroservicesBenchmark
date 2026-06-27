using MediatR;

namespace InventoryService.Application.Queries;

public record ComplexItemCategoryDto(string Id, string Name, ComplexItemCategoryDto? ParentCategory);

public record ComplexItemTranslationDto(string LanguageCode, string Title, string Description);

public record ComplexItemMetadataDto(string Manufacturer, int Weight, bool IsAvailable, string Tags);

public record ComplexItemReviewDto(string Reviewer, int Rating, string Comment, string Date);

public record ComplexItemDto(
    string Id,
    string Name,
    double Price,
    ComplexItemCategoryDto Category,
    List<ComplexItemTranslationDto> Translations,
    ComplexItemMetadataDto Metadata,
    List<ComplexItemReviewDto> Reviews);

public record GetComplexPayloadBenchmarkQuery(int Count) : IRequest<List<ComplexItemDto>>;

public class GetComplexPayloadBenchmarkQueryHandler : IRequestHandler<GetComplexPayloadBenchmarkQuery, List<ComplexItemDto>>
{
    public Task<List<ComplexItemDto>> Handle(GetComplexPayloadBenchmarkQuery request, CancellationToken cancellationToken)
    {
        var items = new List<ComplexItemDto>(request.Count);

        for (int i = 0; i < request.Count; i++)
        {
            var category = new ComplexItemCategoryDto(
                "cat-root-1",
                "Electronics",
                new ComplexItemCategoryDto(
                    "cat-sub-1",
                    "Smartphones",
                    new ComplexItemCategoryDto("cat-leaf-1", "Flagships", null)));

            var translations = new List<ComplexItemTranslationDto>
            {
                new("en-US", $"Smartphone Model {i}", "A very high-end smartphone with excellent features and large memory capacity."),
                new("pl-PL", $"Smartfon Model {i}", "Bardzo zaawansowany smartfon z doskonałymi funkcjami i dużą pamięcią."),
                new("de-DE", $"Smartphone Modell {i}", "Ein sehr hochwertiges Smartphone mit hervorragenden Funktionen und großer Speicherkapazität.")
            };

            var metadata = new ComplexItemMetadataDto(
                "TechCorp Inc.",
                185,
                true,
                "smartphone,5G,oled,120hz,fast-charging");

            var reviews = new List<ComplexItemReviewDto>
            {
                new("user123", 5, "Best phone I've ever had!", "2023-10-01"),
                new("techreviewer", 4, "Great screen, but battery life could be better.", "2023-10-05"),
                new("angry_customer", 2, "Mine came with a scratch on the back.", "2023-10-10"),
                new("happy_buyer", 5, "Fast delivery, product as described.", "2023-10-15")
            };

            items.Add(new ComplexItemDto(
                $"complex-item-{i:D6}",
                $"Complex Item {i}",
                999.99 + (i % 100),
                category,
                translations,
                metadata,
                reviews
            ));
        }

        return Task.FromResult(items);
    }
}
