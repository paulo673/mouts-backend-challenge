using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    : INotificationHandler<SaleCreatedNotification>
{
    public Task Handle(SaleCreatedNotification notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;

        logger.LogInformation(
            "SaleCreated event published. SaleId={SaleId} SaleNumber={SaleNumber} CustomerId={CustomerId} BranchId={BranchId} TotalAmount={TotalAmount} ItemCount={ItemCount}",
            sale.Id,
            sale.SaleNumber,
            sale.CustomerId,
            sale.BranchId,
            sale.TotalAmount,
            sale.Items.Count);

        return Task.CompletedTask;
    }
}
