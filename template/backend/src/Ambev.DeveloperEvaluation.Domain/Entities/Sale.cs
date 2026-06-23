using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public bool IsCancelled { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public List<SaleItem> Items { get; private set; } = [];

    protected Sale()
    {
    }

    public Sale(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        SetHeader(saleNumber, saleDate, customerId, customerName, branchId, branchName);
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        EnsureNotCancelled();
        SetHeader(saleNumber, saleDate, customerId, customerName, branchId, branchName);
        Items.Clear();
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetHeader(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");

        if (branchId == Guid.Empty)
            throw new DomainException("Branch is required.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name is required.");

        SaleNumber = saleNumber.Trim();
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        BranchId = branchId;
        BranchName = branchName.Trim();
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureNotCancelled();
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingItem is null)
        {
            Items.Add(new SaleItem(productId, productName, quantity, unitPrice));
        }
        else
        {
            existingItem.MergeQuantity(quantity, unitPrice);
        }

        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        foreach (var item in Items.Where(i => !i.IsCancelled))
            item.Cancel();

        IsCancelled = true;
        RecalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = Items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new DomainException("Cancelled sale cannot be modified.");
    }
}
