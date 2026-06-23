using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale.TestData;

public static class CreateSaleHandlerTestData
{
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductAId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ProductBId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ProductCId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static CreateSaleItemDto BuildItem(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice) =>
        new()
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

    public static CreateSaleCommand BuildCommand(params CreateSaleItemDto[] items) =>
        new()
        {
            SaleNumber = "SALE-001",
            SaleDate = new DateTime(2026, 6, 23, 10, 30, 0, DateTimeKind.Utc),
            CustomerId = CustomerId,
            CustomerName = "John Customer",
            BranchId = BranchId,
            BranchName = "Manaus Branch",
            Items = items.ToList()
        };

    public static CreateSaleCommand GenerateValidCommand() =>
        BuildCommand(BuildItem(ProductAId, "Product A", 3, 10m));

    public static CreateSaleCommand GenerateCommandWithExactQuantity(int quantity) =>
        BuildCommand(BuildItem(ProductAId, "Product A", quantity, 10m));

    public static CreateSaleCommand GenerateCommandWithMixedQuantityItems() =>
        BuildCommand(
            BuildItem(ProductAId, "Product A", 2, 10m),
            BuildItem(ProductBId, "Product B", 5, 20m),
            BuildItem(ProductCId, "Product C", 10, 30m));

    public static CreateSaleCommand GenerateCommandWithDuplicateProductLines() =>
        BuildCommand(
            BuildItem(ProductAId, "Product A", 2, 10m),
            BuildItem(ProductAId, "Product A", 3, 10m));

    public static CreateSaleCommand GenerateCommandWithDuplicateProductLinesOverLimit() =>
        BuildCommand(
            BuildItem(ProductAId, "Product A", 12, 10m),
            BuildItem(ProductAId, "Product A", 9, 10m));

    public static CreateSaleCommand GenerateCommandWithDuplicateProductDifferentPrice() =>
        BuildCommand(
            BuildItem(ProductAId, "Product A", 2, 10m),
            BuildItem(ProductAId, "Product A", 1, 12m));
}
