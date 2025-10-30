using FluentValidation;

namespace ApiGateway.Core.User.Dto.Validator;

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("FirstName is required")
            .MaximumLength(100)
            .WithMessage("FirstName must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("LastName is required")
            .MaximumLength(100)
            .WithMessage("LastName must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters");
    }
}
