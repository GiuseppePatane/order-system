using FluentValidation;

namespace ApiGateway.Core.Order.Dto.Validator;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequestDto>
{
    private static readonly string[] ValidStatuses =
    {
        "Pending",
        "Confirmed",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeAValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }

    private static bool BeAValidStatus(string status)
    {
        return !string.IsNullOrWhiteSpace(status) &&
               ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
