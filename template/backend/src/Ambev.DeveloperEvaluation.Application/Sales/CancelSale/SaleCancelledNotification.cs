using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class SaleCancelledNotification(SaleCancelledEvent domainEvent) : INotification
{
    public SaleCancelledEvent DomainEvent { get; } = domainEvent;
}
