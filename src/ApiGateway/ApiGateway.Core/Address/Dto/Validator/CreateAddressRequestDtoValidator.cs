using FluentValidation;

namespace ApiGateway.Core.Address.Dto.Validator;

public class CreateAddressRequestDtoValidator : AbstractValidator<CreateAddressRequestDto>
{
    public CreateAddressRequestDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .Must(BeAValidGuid)
            .WithMessage("UserId must be a valid GUID");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.Street2)
            .MaximumLength(200)
            .WithMessage("Street2 must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Street2));

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(100)
            .WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("PostalCode is required")
            .MaximumLength(20)
            .WithMessage("PostalCode must not exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.Label)
            .MaximumLength(50)
            .WithMessage("Label must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Label));
    }

    private static bool BeAValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}
