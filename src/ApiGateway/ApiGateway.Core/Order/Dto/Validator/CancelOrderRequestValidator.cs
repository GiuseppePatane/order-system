using FluentValidation;

namespace ApiGateway.Core.Order.Dto.Validator;

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequestDto>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
    }
}
