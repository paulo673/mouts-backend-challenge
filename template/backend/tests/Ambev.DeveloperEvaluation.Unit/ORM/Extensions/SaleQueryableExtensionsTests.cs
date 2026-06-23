using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Extensions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Extensions;

public class SaleQueryableExtensionsTests
{
    private static IQueryable<Sale> BuildSales()
    {
        var first = new Sale("SALE-003", new DateTime(2026, 1, 1), Guid.NewGuid(), "Alice", Guid.NewGuid(), "Branch A");
        first.AddItem(Guid.NewGuid(), "Product", 1, 10m);

        var second = new Sale("SALE-001", new DateTime(2026, 3, 1), Guid.NewGuid(), "Bob", Guid.NewGuid(), "Branch B");
        second.AddItem(Guid.NewGuid(), "Product", 5, 10m);

        var third = new Sale("SALE-002", new DateTime(2026, 2, 1), Guid.NewGuid(), "Carol", Guid.NewGuid(), "Branch C");
        third.AddItem(Guid.NewGuid(), "Product", 1, 10m);

        return new[] { first, second, third }.AsQueryable();
    }

    [Fact(DisplayName = "Given no order When applying ordering Then orders by sale date descending by default")]
    public void ApplyOrdering_NoOrder_OrdersBySaleDateDescending()
    {
        var result = BuildSales().ApplyOrdering(null).ToList();

        result.Select(s => s.SaleNumber).Should().ContainInOrder("SALE-001", "SALE-002", "SALE-003");
    }

    [Fact(DisplayName = "Given single field ascending When applying ordering Then orders by that field ascending")]
    public void ApplyOrdering_SingleFieldAscending_OrdersAscending()
    {
        var result = BuildSales().ApplyOrdering("saleNumber asc").ToList();

        result.Select(s => s.SaleNumber).Should().ContainInOrder("SALE-001", "SALE-002", "SALE-003");
    }

    [Fact(DisplayName = "Given composite order When applying ordering Then applies primary and secondary ordering")]
    public void ApplyOrdering_CompositeOrder_AppliesThenBy()
    {
        var result = BuildSales().ApplyOrdering("totalAmount desc, saleNumber asc").ToList();

        result.First().SaleNumber.Should().Be("SALE-001");
    }

    [Fact(DisplayName = "Given unknown field When applying ordering Then falls back to default ordering")]
    public void ApplyOrdering_UnknownField_FallsBackToDefault()
    {
        var result = BuildSales().ApplyOrdering("unknownField asc").ToList();

        result.Select(s => s.SaleNumber).Should().ContainInOrder("SALE-001", "SALE-002", "SALE-003");
    }
}
