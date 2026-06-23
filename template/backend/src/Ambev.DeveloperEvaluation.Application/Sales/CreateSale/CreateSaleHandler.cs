using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler(ISaleRepository saleRepository, IPublisher publisher, ILogger<CreateSaleHandler> logger)
    : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = new Sale(
            command.SaleNumber,
            command.SaleDate,
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);

        foreach (var item in command.Items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

        var created = await saleRepository.CreateAsync(sale, cancellationToken);

        logger.LogInformation(
            "Sale created with {ItemCount} item lines. SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount}",
            created.Items.Count,
            created.Id,
            created.SaleNumber,
            created.TotalAmount);

        await publisher.Publish(new SaleCreatedNotification(new SaleCreatedEvent(created)), cancellationToken);

        return new CreateSaleResult
        {
            Id = created.Id,
            SaleNumber = created.SaleNumber,
            TotalAmount = created.TotalAmount,
            CreatedAt = created.CreatedAt
        };
    }
}
