using FluentValidation;

namespace ApiGateway.Core.Address.Dto.Validator;

public class UpdateAddressRequestDtoValidator : AbstractValidator<UpdateAddressRequestDto>
{
    public UpdateAddressRequestDtoValidator()
    {

        RuleFor(x => x.Street)
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Street));

        RuleFor(x => x.Street2)
            .MaximumLength(200)
            .WithMessage("Street2 must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Street2));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100)
            .WithMessage("State must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage("PostalCode must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.Label)
            .MaximumLength(50)
            .WithMessage("Label must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Label));

        RuleFor(x => x)
            .Must(HaveAtLeastOneProperty)
            .WithMessage("At least one property must be provided for update");
    }

    private static bool BeAValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    private static bool HaveAtLeastOneProperty(UpdateAddressRequestDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Street) ||
               !string.IsNullOrWhiteSpace(dto.Street2) ||
               !string.IsNullOrWhiteSpace(dto.City) ||
               !string.IsNullOrWhiteSpace(dto.State) ||
               !string.IsNullOrWhiteSpace(dto.PostalCode) ||
               !string.IsNullOrWhiteSpace(dto.Country) ||
               !string.IsNullOrWhiteSpace(dto.Label);
    }
}
