using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemRequestValidator : AbstractValidator<CancelSaleItemRequest>
{
    public CancelSaleItemRequestValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ItemId)
            .NotEqual(Guid.Empty);
    }
}
