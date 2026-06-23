using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    protected SaleItem()
    {
    }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product is required.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        ProductId = productId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;

        ApplyDiscountRules();
    }

    public void MergeQuantity(int additionalQuantity, decimal unitPrice)
    {
        if (additionalQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice != UnitPrice)
            throw new DomainException("Unit price must be consistent for the same product.");

        Quantity += additionalQuantity;
        ApplyDiscountRules();
    }

    private void ApplyDiscountRules()
    {
        if (Quantity > 20)
            throw new DomainException("Cannot sell more than 20 identical items.");

        Discount = Quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };

        TotalAmount = Quantity * UnitPrice * (1 - Discount);
    }
}