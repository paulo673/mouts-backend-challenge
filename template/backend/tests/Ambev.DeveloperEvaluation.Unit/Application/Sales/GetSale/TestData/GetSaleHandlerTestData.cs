using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSale.TestData;

public static class GetSaleHandlerTestData
{
    public static readonly Guid SaleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductAId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProductBId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static GetSaleQuery BuildQuery() => new(SaleId);

    public static Sale BuildSale()
    {
        var sale = new Sale(
            "SALE-001",
            new DateTime(2026, 6, 23, 10, 30, 0, DateTimeKind.Utc),
            CustomerId,
            "John Customer",
            BranchId,
            "Manaus Branch")
        {
            Id = SaleId
        };

        sale.AddItem(ProductAId, "Product A", 4, 10m);
        sale.AddItem(ProductBId, "Product B", 2, 20m);

        return sale;
    }
}
