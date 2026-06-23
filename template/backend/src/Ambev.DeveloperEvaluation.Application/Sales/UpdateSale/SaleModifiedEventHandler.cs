using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    : INotificationHandler<SaleModifiedNotification>
{
    public Task Handle(SaleModifiedNotification notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;

        logger.LogInformation(
            "SaleModified event published. SaleId={SaleId} SaleNumber={SaleNumber} CustomerId={CustomerId} BranchId={BranchId} TotalAmount={TotalAmount} ItemCount={ItemCount}",
            sale.Id,
            sale.SaleNumber,
            sale.CustomerId,
            sale.BranchId,
            sale.TotalAmount,
            sale.Items.Count);

        return Task.CompletedTask;
    }
}
