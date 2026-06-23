using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class SaleModifiedNotification(SaleModifiedEvent domainEvent) : INotification
{
    public SaleModifiedEvent DomainEvent { get; } = domainEvent;
}
