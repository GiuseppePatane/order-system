using FluentValidation;

namespace ApiGateway.Core.Product.Dto.Validator;

public class UpdateProductRequestDtoValidator : AbstractValidator<UpdateProductRequestDto>
{
    public UpdateProductRequestDtoValidator()
    {
       

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Sku)
            .MaximumLength(50)
            .WithMessage("SKU must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Sku));

        RuleFor(x => x.CategoryId)
            .Must(BeAValidGuid)
            .WithMessage("CategoryId must be a valid GUID")
            .When(x => !string.IsNullOrWhiteSpace(x.CategoryId));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }

    private static bool BeAValidGuid(string? value)
    {
        return Guid.TryParse(value, out _);
    }
}
