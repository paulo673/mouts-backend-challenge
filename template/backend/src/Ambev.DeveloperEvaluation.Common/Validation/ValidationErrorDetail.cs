using FluentValidation.Results;

namespace Ambev.DeveloperEvaluation.Common.Validation;

public class ValidationErrorDetail
{
    public string Type { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;

    public static explicit operator ValidationErrorDetail(ValidationFailure validationFailure)
    {
        return new ValidationErrorDetail
        {
            Type = "ValidationError",
            Detail = validationFailure.ErrorMessage,
            Error = validationFailure.ErrorCode
        };
    }
}
