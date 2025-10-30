namespace ApiGateway.Api.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidationFilter : TypeFilterAttribute
{
    public ValidationFilter() : base(typeof(ValidationHandlerFilter))
    {
    }

    private class ValidationHandlerFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };
                var invalidProps = context.ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Any())
                    .Select(x => new 
                    {
                        Field = x.Key,
                        Errors = x.Value?.Errors.Select(e => e.ErrorMessage).ToList(),
                        AttemptedValue = x.Value?.RawValue
                    })
                    .ToList();

                problemDetails.Extensions["invalidProperties"] = invalidProps;
                context.Result = new BadRequestObjectResult(problemDetails);
            }
        }
    }
}
