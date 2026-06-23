using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSale.TestData;

public static class CancelSaleHandlerTestData
{
    public static readonly Guid SaleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductAId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProductBId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static CancelSaleCommand GenerateValidCommand() =>
        new()
        {
            Id = SaleId
        };

    public static Sale GenerateExistingSale()
    {
        var sale = new Sale(
            "SALE-001",
            new DateTime(2026, 6, 23, 10, 30, 0, DateTimeKind.Utc),
            CustomerId,
            "Original Customer",
            BranchId,
            "Original Branch")
        {
            Id = SaleId
        };

        sale.AddItem(ProductAId, "Product A", 2, 10m);
        sale.AddItem(ProductBId, "Product B", 1, 5m);

        return sale;
    }

    public static Sale GenerateCancelledSale()
    {
        var sale = GenerateExistingSale();
        sale.Cancel();
        return sale;
    }
}
