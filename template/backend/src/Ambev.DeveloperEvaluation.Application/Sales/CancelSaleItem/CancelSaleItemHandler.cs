using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler(ISaleRepository saleRepository, IPublisher publisher, ILogger<CancelSaleItemHandler> logger)
    : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        sale.CancelItem(command.ItemId);

        var updated = await saleRepository.UpdateAsync(sale, cancellationToken);

        var cancelledItem = updated.Items.Single(item => item.Id == command.ItemId);

        logger.LogInformation(
            "Sale item cancelled. SaleId={SaleId} ItemId={ItemId} TotalAmount={TotalAmount}",
            updated.Id,
            command.ItemId,
            updated.TotalAmount);

        await publisher.Publish(new ItemCancelledNotification(new ItemCancelledEvent(updated, cancelledItem)), cancellationToken);

        return new CancelSaleItemResult
        {
            SaleId = updated.Id,
            ItemId = command.ItemId,
            IsCancelled = cancelledItem.IsCancelled,
            TotalAmount = updated.TotalAmount,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
