using ApiGateway.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

public class Base : ControllerBase
{
    private readonly ILogger<Base> _logger;

    public Base(ILogger<Base> logger)
    {
        _logger = logger;
    }

    protected IActionResult MapErrorToProblemDetails(ErrorInfo error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Extensions = { ["errorCode"] = error.Code },
        };

        if (error.Details != null && error.Details.Count > 0)
        {
            problemDetails.Extensions["additionalDetails"] = error.Details;
        }

        var (statusCode, type) = error.Code switch
        {
            //todo: magic numbers to constants
            // Address
            "ADDRESS_NOT_FOUND" or "NOT_FOUND" => (
                StatusCodes.Status404NotFound,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            ),
            "SHIPPING_ADDRESS_USER_MISMATCH"=> (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            "INVALID_ADDRESS_ID" => (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            // Order
            "PRODUCT_VALIDATION_FAILED"
            or "INVALID_ORDER_DATA"
            or "PRODUCT_ALREADY_IN_ORDER"
            or "INVALID_QUANTITY"
            or "STOCK_LOCK_FAILED" => (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            "ORDERITEM_NOT_FOUND" => (
                StatusCodes.Status404NotFound,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            ),
            "ORDER_CREATION_FAILED" or "ADD_ITEM_FAILED" => (
                StatusCodes.Status500InternalServerError,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            ),
            // product

            "INVALID_PRODUCT_ID" => (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            // User
            "USER_NOT_FOUND" => (
                StatusCodes.Status404NotFound,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            ),
            "INVALID_USER_ID" => (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),

            // common
            "ORDER_NOT_FOUND" or "USER_NOT_FOUND" or "PRODUCT_NOT_FOUND" => (
                StatusCodes.Status404NotFound,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
            ),
            "GRPC_ERROR" => (
                StatusCodes.Status503ServiceUnavailable,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4"
            ),
            "EMPTY_RESPONSE" => (
                StatusCodes.Status502BadGateway,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3"
            ),
            "INVALID_ARGUMENT" => (
                StatusCodes.Status400BadRequest,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            ),
        };

        problemDetails.Status = statusCode;
        problemDetails.Type = type;

        _logger.LogWarning(
            "Returning error response: {Code} with HTTP status {StatusCode}",
            error.Code,
            statusCode
        );

        return StatusCode(statusCode, problemDetails);
    }
}
