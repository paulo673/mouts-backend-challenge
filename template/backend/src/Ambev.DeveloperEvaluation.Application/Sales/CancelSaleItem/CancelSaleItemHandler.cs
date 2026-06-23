using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler(ISaleRepository saleRepository, ILogger<CancelSaleItemHandler> logger)
    : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        sale.CancelItem(command.ItemId);

        var updated = await saleRepository.UpdateAsync(sale, cancellationToken);

        logger.LogInformation(
            "Sale item cancelled. SaleId={SaleId} ItemId={ItemId} TotalAmount={TotalAmount}",
            updated.Id,
            command.ItemId,
            updated.TotalAmount);

        return new CancelSaleItemResult
        {
            SaleId = updated.Id,
            ItemId = command.ItemId,
            IsCancelled = updated.Items.Single(item => item.Id == command.ItemId).IsCancelled,
            TotalAmount = updated.TotalAmount,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
