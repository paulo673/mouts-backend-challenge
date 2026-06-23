using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSales.TestData;

public static class GetSalesHandlerTestData
{
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductAId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static GetSalesQuery BuildQuery(int page = 1, int size = 10, string? order = null) =>
        new() { Page = page, Size = size, Order = order };

    public static Sale BuildSale(string saleNumber)
    {
        var sale = new Sale(
            saleNumber,
            new DateTime(2026, 6, 23, 10, 30, 0, DateTimeKind.Utc),
            CustomerId,
            "John Customer",
            BranchId,
            "Manaus Branch");

        sale.AddItem(ProductAId, "Product A", 4, 10m);

        return sale;
    }

    public static IReadOnlyList<Sale> BuildSales(int count)
    {
        var sales = new List<Sale>();
        for (var i = 1; i <= count; i++)
            sales.Add(BuildSale($"SALE-{i:000}"));

        return sales;
    }
}
