using FluentValidation;

namespace ApiGateway.Core.User.Dto.Validator;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("FirstName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("LastName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x)
            .Must(HaveAtLeastOneProperty)
            .WithMessage("At least one property must be provided for update");
    }

    private static bool HaveAtLeastOneProperty(UpdateUserRequestDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.FirstName) ||
               !string.IsNullOrWhiteSpace(dto.LastName) ||
               !string.IsNullOrWhiteSpace(dto.Email);
    }
}
