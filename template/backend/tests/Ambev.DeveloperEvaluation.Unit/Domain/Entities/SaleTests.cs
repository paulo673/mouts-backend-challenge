using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Given active sale When cancelling Then marks sale as cancelled")]
    public void Cancel_ActiveSale_MarksSaleAsCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given active sale When cancelling Then cancels all items")]
    public void Cancel_ActiveSale_CancelsAllItems()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.Items.Should().OnlyContain(item => item.IsCancelled);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then recalculates total amount to zero")]
    public void Cancel_ActiveSale_RecalculatesTotalAmountToZero()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.TotalAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then updates updated at")]
    public void Cancel_ActiveSale_UpdatesUpdatedAt()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.UpdatedAt.Should().NotBeNull();
        sale.UpdatedAt.Should().BeOnOrAfter(sale.CreatedAt);
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling again Then throws domain exception")]
    public void Cancel_AlreadyCancelledSale_ThrowsDomainException()
    {
        var sale = CreateSale();
        sale.Cancel();

        var act = sale.Cancel;

        act.Should().Throw<DomainException>()
            .WithMessage("Sale is already cancelled.");
    }

    [Fact(DisplayName = "Given cancelled sale When adding item Then throws domain exception")]
    public void AddItem_CancelledSale_ThrowsDomainException()
    {
        var sale = CreateSale();
        sale.Cancel();

        var act = () => sale.AddItem(Guid.NewGuid(), "Product C", 1, 5m);

        act.Should().Throw<DomainException>()
            .WithMessage("Cancelled sale cannot be modified.");
    }

    private static Sale CreateSale()
    {
        var sale = new Sale(
            "SALE-001",
            new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Customer",
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Branch");

        sale.AddItem(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Product A", 2, 10m);
        sale.AddItem(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Product B", 1, 20m);

        return sale;
    }
}
