using FluentValidation;

namespace ApiGateway.Core.Order.Dto.Validator;

public class CreateOrderRequestDtoValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.ShippingAddressId)
            .NotEmpty()
            .WithMessage("ShippingAddressId is required");

        RuleFor(x => x.FirstItem)
            .NotNull()
            .WithMessage("Items list is required")
            .NotEmpty()
            .WithMessage("At least one item is required in the order");
        

        RuleFor(x => x.FirstItem)
            .SetValidator(new OrderItemDtoValidator());
    }
}
