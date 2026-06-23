using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler(ISaleRepository saleRepository, IPublisher publisher, ILogger<UpdateSaleHandler> logger)
    : IRequestHandler<UpdateSaleCommand, UpdateSaleResult?>
{
    public async Task<UpdateSaleResult?> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale is null)
            return null;

        sale.Update(
            command.SaleNumber,
            command.SaleDate,
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);

        foreach (var item in command.Items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

        var updated = await saleRepository.UpdateAsync(sale, cancellationToken);

        logger.LogInformation(
            "Sale updated with {ItemCount} item lines. SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount}",
            updated.Items.Count,
            updated.Id,
            updated.SaleNumber,
            updated.TotalAmount);

        await publisher.Publish(new SaleModifiedNotification(new SaleModifiedEvent(updated)), cancellationToken);

        return new UpdateSaleResult
        {
            Id = updated.Id,
            SaleNumber = updated.SaleNumber,
            TotalAmount = updated.TotalAmount,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
