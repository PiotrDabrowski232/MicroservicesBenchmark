namespace OrderService.Application.DTOs;

public record ComplexItemCategoryDto(string Id, string Name, ComplexItemCategoryDto? ParentCategory);

public record ComplexItemTranslationDto(string LanguageCode, string Title, string Description);

public record ComplexItemMetadataDto(string Manufacturer, int Weight, bool IsAvailable, string Tags);

public record ComplexItemReviewDto(string Reviewer, int Rating, string Comment, string Date);

public record BenchmarkComplexItemDto(
    string Id,
    string Name,
    double Price,
    ComplexItemCategoryDto Category,
    List<ComplexItemTranslationDto> Translations,
    ComplexItemMetadataDto Metadata,
    List<ComplexItemReviewDto> Reviews);
