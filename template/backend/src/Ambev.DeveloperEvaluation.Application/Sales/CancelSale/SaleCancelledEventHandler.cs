using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    : INotificationHandler<SaleCancelledNotification>
{
    public Task Handle(SaleCancelledNotification notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;

        logger.LogInformation(
            "SaleCancelled event published. SaleId={SaleId} SaleNumber={SaleNumber} CustomerId={CustomerId} BranchId={BranchId} TotalAmount={TotalAmount} ItemCount={ItemCount}",
            sale.Id,
            sale.SaleNumber,
            sale.CustomerId,
            sale.BranchId,
            sale.TotalAmount,
            sale.Items.Count);

        return Task.CompletedTask;
    }
}
