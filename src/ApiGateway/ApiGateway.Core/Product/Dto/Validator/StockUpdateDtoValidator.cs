using FluentValidation;

namespace ApiGateway.Core.Product.Dto.Validator;

public class StockUpdateDtoValidator : AbstractValidator<StockUpdateDto>
{
    public StockUpdateDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required")
            .Must(BeAValidGuid)
            .WithMessage("ProductId must be a valid GUID");

        RuleFor(x => x.UpdatedStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("UpdatedStock must be greater than or equal to zero");

        RuleFor(x => x.LockedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LockedQuantity must be greater than or equal to zero");

        RuleFor(x => x.LockedPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LockedPrice must be greater than or equal to zero");
    }

    private static bool BeAValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}
