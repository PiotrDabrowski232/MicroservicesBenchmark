using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OrderService.Domain.Models;

using SharedKernel.VariablesExtensions;

namespace OrderService.Infrastructure.ModelsConfiguration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasData(_products);
        }

        private readonly Product[] _products = new[]
         {
            new Product
            {
                Id = IntExtensions.ToGuid(1),
                Name = "Konsultacja techniczna (30 min)",
                Price = 150.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(2),
                Name = "Konsultacja techniczna (60 min)",
                Price = 250.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(3),
                Name = "Analiza wymagań biznesowych",
                Price = 800.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(4),
                Name = "Projekt architektury systemu",
                Price = 1800.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(5),
                Name = "Implementacja funkcjonalności (1h)",
                Price = 220.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(6),
                Name = "Refaktoryzacja kodu (1h)",
                Price = 200.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(7),
                Name = "Code review",
                Price = 300.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(8),
                Name = "Audyt techniczny aplikacji",
                Price = 2500.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(9),
                Name = "Optymalizacja wydajności",
                Price = 1200.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(10),
                Name = "Integracja z API zewnętrznym",
                Price = 900.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(11),
                Name = "Konfiguracja środowiska produkcyjnego",
                Price = 700.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(12),
                Name = "Konfiguracja CI/CD",
                Price = 1400.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(13),
                Name = "Wdrożenie aplikacji",
                Price = 600.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(14),
                Name = "Monitoring i logowanie (setup)",
                Price = 850.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(15),
                Name = "Wsparcie techniczne (1h)",
                Price = 180.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(16),
                Name = "Pakiet wsparcia miesięcznego",
                Price = 2200.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(17),
                Name = "Migracja bazy danych",
                Price = 1600.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(18),
                Name = "Backup i strategia odzyskiwania danych",
                Price = 950.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(19),
                Name = "Szkolenie techniczne zespołu",
                Price = 1300.00m
            },
            new Product
            {
                Id = IntExtensions.ToGuid(20),
                Name = "Warsztat architektoniczny",
                Price = 2000.00m
            }
        };
    }
}
