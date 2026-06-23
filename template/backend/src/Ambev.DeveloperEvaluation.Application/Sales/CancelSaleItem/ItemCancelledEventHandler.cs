using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    : INotificationHandler<ItemCancelledNotification>
{
    public Task Handle(ItemCancelledNotification notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;
        var item = notification.DomainEvent.Item;

        logger.LogInformation(
            "ItemCancelled event published. SaleId={SaleId} SaleNumber={SaleNumber} ItemId={ItemId} ProductId={ProductId} Quantity={Quantity} TotalAmount={TotalAmount}",
            sale.Id,
            sale.SaleNumber,
            item.Id,
            item.ProductId,
            item.Quantity,
            sale.TotalAmount);

        return Task.CompletedTask;
    }
}
