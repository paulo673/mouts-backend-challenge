using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler(ISaleRepository saleRepository, IPublisher publisher, ILogger<CancelSaleHandler> logger)
    : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        sale.Cancel();

        var updated = await saleRepository.UpdateAsync(sale, cancellationToken);

        logger.LogInformation(
            "Sale cancelled. SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount}",
            updated.Id,
            updated.SaleNumber,
            updated.TotalAmount);

        await publisher.Publish(new SaleCancelledNotification(new SaleCancelledEvent(updated)), cancellationToken);

        return new CancelSaleResult
        {
            Id = updated.Id,
            SaleNumber = updated.SaleNumber,
            IsCancelled = updated.IsCancelled,
            TotalAmount = updated.TotalAmount,
            UpdatedAt = updated.UpdatedAt
        };
    }
}
