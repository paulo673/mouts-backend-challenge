using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class SaleCreatedNotification(SaleCreatedEvent domainEvent) : INotification
{
    public SaleCreatedEvent DomainEvent { get; } = domainEvent;
}
