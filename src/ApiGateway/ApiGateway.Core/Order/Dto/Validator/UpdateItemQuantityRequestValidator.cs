using FluentValidation;

namespace ApiGateway.Core.Order.Dto.Validator;

public class UpdateItemQuantityRequestValidator : AbstractValidator<UpdateItemQuantityRequestDto>
{
    public UpdateItemQuantityRequestValidator()
    {
        RuleFor(x => x.NewQuantity)
            .GreaterThan(0)
            .WithMessage("NewQuantity must be greater than zero");
    }
}
