using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ItemId)
            .NotEqual(Guid.Empty);
    }
}
