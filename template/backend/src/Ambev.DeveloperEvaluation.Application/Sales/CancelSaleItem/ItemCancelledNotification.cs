using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class ItemCancelledNotification(ItemCancelledEvent domainEvent) : INotification
{
    public ItemCancelledEvent DomainEvent { get; } = domainEvent;
}
