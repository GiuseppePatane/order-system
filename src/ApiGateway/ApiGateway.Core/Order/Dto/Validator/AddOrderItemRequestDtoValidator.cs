using FluentValidation;

namespace ApiGateway.Core.Order.Validator;

public class AddOrderItemRequestDtoValidator : AbstractValidator<AddOrderItemRequestDto>
{
    public AddOrderItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}
