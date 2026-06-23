using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

public static class SaleQueryableExtensions
{
    private static readonly Dictionary<string, Expression<Func<Sale, object>>> Selectors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["saleNumber"] = s => s.SaleNumber,
            ["saleDate"] = s => s.SaleDate,
            ["customerName"] = s => s.CustomerName,
            ["branchName"] = s => s.BranchName,
            ["totalAmount"] = s => s.TotalAmount,
            ["createdAt"] = s => s.CreatedAt
        };

    public static IQueryable<Sale> ApplyOrdering(this IQueryable<Sale> query, string? order)
    {
        IOrderedQueryable<Sale>? ordered = null;

        if (!string.IsNullOrWhiteSpace(order))
        {
            foreach (var token in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!Selectors.TryGetValue(parts[0], out var selector))
                    continue;

                var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

                if (ordered is null)
                    ordered = descending ? query.OrderByDescending(selector) : query.OrderBy(selector);
                else
                    ordered = descending ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector);
            }
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }
}
