using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSaleItem.TestData;

public static class CancelSaleItemHandlerTestData
{
    public static readonly Guid SaleId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductAId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProductBId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ItemAId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid ItemBId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid UnknownItemId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    public static CancelSaleItemCommand GenerateValidCommand() =>
        new()
        {
            SaleId = SaleId,
            ItemId = ItemAId
        };

    public static CancelSaleItemCommand GenerateCommandWithUnknownItem() =>
        new()
        {
            SaleId = SaleId,
            ItemId = UnknownItemId
        };

    public static CancelSaleItemCommand GenerateCommandWithInvalidSaleId() =>
        new()
        {
            SaleId = Guid.Empty,
            ItemId = ItemAId
        };

    public static CancelSaleItemCommand GenerateCommandWithInvalidItemId() =>
        new()
        {
            SaleId = SaleId,
            ItemId = Guid.Empty
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
        sale.AddItem(ProductBId, "Product B", 1, 20m);
        sale.Items[0].Id = ItemAId;
        sale.Items[1].Id = ItemBId;

        return sale;
    }

    public static Sale GenerateSaleWithCancelledItem()
    {
        var sale = GenerateExistingSale();
        sale.CancelItem(ItemAId);
        return sale;
    }
}
