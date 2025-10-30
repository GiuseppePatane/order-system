using FluentValidation;

namespace ApiGateway.Core.Product.Dto.Validator;

public class CreateProductRequestDtoValidator : AbstractValidator<CreateProductRequestDto>
{
    public CreateProductRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock must be greater than or equal to zero");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(50)
            .WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required")
            .Must(BeAValidGuid)
            .WithMessage("CategoryId must be a valid GUID");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }

    private static bool BeAValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}