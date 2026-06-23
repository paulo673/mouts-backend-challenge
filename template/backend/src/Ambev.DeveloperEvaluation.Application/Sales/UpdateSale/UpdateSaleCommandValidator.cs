using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEqual(Guid.Empty);

        RuleFor(c => c.SaleNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.SaleDate)
            .NotEqual(default(DateTime));

        RuleFor(c => c.CustomerId)
            .NotEqual(Guid.Empty);

        RuleFor(c => c.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.BranchId)
            .NotEqual(Guid.Empty);

        RuleFor(c => c.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Items)
            .NotEmpty();

        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEqual(Guid.Empty);
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(100);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
