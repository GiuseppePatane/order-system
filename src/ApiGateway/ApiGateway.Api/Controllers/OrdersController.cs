using ApiGateway.Core.Order;
using ApiGateway.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderOrchestrationService _orderOrchestration;
    private readonly IOrderServiceClient _orderClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderOrchestrationService orderOrchestration,
        IOrderServiceClient orderClient,
        ILogger<OrdersController> logger)
    {
        _orderOrchestration = orderOrchestration;
        _orderClient = orderClient;
        _logger = logger;
    }

    /*
     Create a new order
     
     curl example:
     curl -X POST http://localhost:5000/api/orders \
       -H "Content-Type: application/json" \
       -d '{
         "userId": "11111111-1111-1111-1111-111111111111",
         "shippingAddressId": "22222222-2222-2222-2222-222222222222",
         "items": [
           {
             "productId": "33333333-3333-3333-3333-333333333333",
             "quantity": 2,
             "unitPrice": 29.99
           }
         ]
       }'
    */
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        _logger.LogInformation("Creating order for user {UserId}", request.UserId);

        var result = await _orderOrchestration.CreateOrderAsync(request);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetOrder), 
                new { id = result.Data.OrderId }, 
                result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Get order by ID
     
     curl example:
     curl http://localhost:5000/api/orders/{orderId}
    */
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrder(string id)
    {
        var result = await _orderOrchestration.GetOrderByIdAsync(id);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Get orders for a specific user
     
     curl example:
     curl http://localhost:5000/api/orders/user/{userId}
    */
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserOrders(string userId)
    {
        var result = await _orderOrchestration.GetUserOrdersAsync(userId);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Cancel an order
     
     curl example:
     curl -X DELETE http://localhost:5000/api/orders/{orderId}/cancel \
       -H "Content-Type: application/json" \
       -d '{"reason": "Customer requested cancellation"}'
    */
    [HttpDelete("{id}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest? request)
    {
        var result = await _orderOrchestration.CancelOrderAsync(id, request?.Reason);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Update order status
     
     curl example:
     curl -X PATCH http://localhost:5000/api/orders/{orderId}/status \
       -H "Content-Type: application/json" \
       -d '{"status": "Confirmed"}'
    */
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderOrchestration.UpdateOrderStatusAsync(id, request.Status);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Add an item to an existing order (only for pending orders)

     curl example:
     curl -X POST http://localhost:5000/api/orders/{orderId}/items \
       -H "Content-Type: application/json" \
       -d '{
         "productId": "33333333-3333-3333-3333-333333333333",
         "quantity": 1,
         "unitPrice": 19.99
       }'
    */
    [HttpPost("{orderId}/items")]
    [ProducesResponseType(typeof(AddOrderItemResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddOrderItem(string orderId, [FromBody] AddOrderItemRequestDto request)
    {
        _logger.LogInformation("Adding item to order {OrderId}", orderId);

        var result = await _orderClient.AddOrderItem(orderId, request);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Remove an item from an existing order (only for pending orders)

     curl example:
     curl -X DELETE http://localhost:5000/api/orders/{orderId}/items/{itemId}
    */
    [HttpDelete("{orderId}/items/{itemId}")]
    [ProducesResponseType(typeof(RemoveOrderItemResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveOrderItem(string orderId, string itemId)
    {
        _logger.LogInformation("Removing item {ItemId} from order {OrderId}", itemId, orderId);

        var result = await _orderClient.RemoveOrderItem(orderId, itemId);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /*
     Update the quantity of an order item (only for pending orders)

     curl example:
     curl -X PATCH http://localhost:5000/api/orders/{orderId}/items/{itemId}/quantity \
       -H "Content-Type: application/json" \
       -d '{"newQuantity": 5}'
    */
    [HttpPatch("{orderId}/items/{itemId}/quantity")]
    [ProducesResponseType(typeof(UpdateOrderItemQuantityResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderItemQuantity(string orderId, string itemId, [FromBody] UpdateItemQuantityRequest request)
    {
        _logger.LogInformation("Updating item {ItemId} quantity in order {OrderId} to {NewQuantity}",
            itemId, orderId, request.NewQuantity);

        var result = await _orderClient.UpdateOrderItemQuantity(orderId, itemId, request.NewQuantity);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    private IActionResult MapErrorToProblemDetails(ApiGateway.Core.Common.ErrorInfo error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Extensions = { ["errorCode"] = error.Code }
        };

        if (error.Details != null && error.Details.Count > 0)
        {
            problemDetails.Extensions["additionalDetails"] = error.Details;
        }

        var (statusCode, type) = error.Code switch
        {
            "ORDER_NOT_FOUND" or "USER_NOT_FOUND" => 
                (StatusCodes.Status404NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),
            "PRODUCT_VALIDATION_FAILED" or "INVALID_ORDER_DATA" => 
                (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
           "INVALID_ARGUMENT" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "ORDER_CREATION_FAILED" => 
                (StatusCodes.Status500InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"),
            _ => (StatusCodes.Status500InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };

        problemDetails.Status = statusCode;
        problemDetails.Type = type;

        _logger.LogWarning("Returning error response: {Code} with HTTP status {StatusCode}", 
            error.Code, statusCode);

        return StatusCode(statusCode, problemDetails);
    }
}

public record CancelOrderRequest(string? Reason);
public record UpdateOrderStatusRequest(string Status);
public record UpdateItemQuantityRequest(int NewQuantity);

