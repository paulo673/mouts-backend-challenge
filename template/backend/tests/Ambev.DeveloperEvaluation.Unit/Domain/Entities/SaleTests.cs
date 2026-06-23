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

    [Fact(DisplayName = "Given active sale When cancelling item Then marks item as cancelled")]
    public void CancelItem_ActiveItem_MarksItemAsCancelled()
    {
        var sale = CreateSale();
        var item = sale.Items[0];

        sale.CancelItem(item.Id);

        item.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given active sale When cancelling item Then recalculates total amount")]
    public void CancelItem_ActiveItem_RecalculatesTotalAmount()
    {
        var sale = CreateSale();
        var item = sale.Items[0];

        sale.CancelItem(item.Id);

        sale.TotalAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "Given active sale When cancelling item Then updates updated at")]
    public void CancelItem_ActiveItem_UpdatesUpdatedAt()
    {
        var sale = CreateSale();
        var item = sale.Items[0];

        sale.CancelItem(item.Id);

        sale.UpdatedAt.Should().NotBeNull();
        sale.UpdatedAt.Should().BeOnOrAfter(sale.CreatedAt);
    }

    [Fact(DisplayName = "Given non-existent item When cancelling item Then throws domain exception")]
    public void CancelItem_ItemNotFound_ThrowsDomainException()
    {
        var sale = CreateSale();

        var act = () => sale.CancelItem(Guid.Parse("55555555-5555-5555-5555-555555555555"));

        act.Should().Throw<DomainException>()
            .WithMessage("Sale item not found.");
    }

    [Fact(DisplayName = "Given already cancelled item When cancelling item Then throws domain exception")]
    public void CancelItem_AlreadyCancelledItem_ThrowsDomainException()
    {
        var sale = CreateSale();
        var item = sale.Items[0];
        sale.CancelItem(item.Id);

        var act = () => sale.CancelItem(item.Id);

        act.Should().Throw<DomainException>()
            .WithMessage("Sale item is already cancelled.");
    }

    [Fact(DisplayName = "Given active sale When cancelling item Then does not alter other items")]
    public void CancelItem_ActiveItem_DoesNotAlterOtherItems()
    {
        var sale = CreateSale();
        var cancelledItem = sale.Items[0];
        var remainingItem = sale.Items[1];

        sale.CancelItem(cancelledItem.Id);

        remainingItem.IsCancelled.Should().BeFalse();
        remainingItem.TotalAmount.Should().Be(20m);
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
        sale.Items[0].Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        sale.Items[1].Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        return sale;
    }
}
