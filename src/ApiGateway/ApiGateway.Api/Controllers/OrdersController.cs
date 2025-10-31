using ApiGateway.Core.Order;
using ApiGateway.Core.Order.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : Base
{
    private readonly IOrderOrchestrationService _orderOrchestration;
    private readonly IOrderServiceClient _orderClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderOrchestrationService orderOrchestration,
        IOrderServiceClient orderClient,
        ILogger<OrdersController> logger) : base(logger)
    {
        _orderOrchestration = orderOrchestration;
        _orderClient = orderClient;
        _logger = logger;
    }


    /// <summary>
    ///  Create a new order with one item
    /// </summary>
    /// <param name="request">The order creation request <see cref="CreateOrderRequestDto"/></param>
    /// <returns>The created  order id </returns>
    /// <response code="200">Order created</response>
    /// <response code="400">Invalid order data</response>
    /// <response code="500">Internal server error during order creation</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderMutationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Returns a single order by its ID
    /// </summary>
    /// <param name="id">the order identifier</param>
    /// <returns> the order information </returns>
    /// <response code="200">Order information <see cref="OrderDto"/></response>
    /// <response code="404">Order not foud</response>
    /// <response code="500">Internal server error during order retrival</response>
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


    /// <summary>
    ///  Get all orders for a specific user
    /// </summary>
    /// <param name="userId">the user id</param>
    /// <returns>A list of order for the  given userid </returns>
    /// <response code="200">List of orders <see cref="List{OrderDto}"/></response>
    /// <response code="404">No orders found for the given user</response>
    /// <response code="500">Internal server error during order retrival</response>
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


    /// <summary>
    ///  Cancel an existing order
    /// </summary>
    /// <param name="id"> The order id to cancel </param>
    /// <param name="request"> the cancel reason </param>
    /// <returns> The deleted order id </returns>
    ///  <response code="200">Order cancelled <see cref="OrderMutationResponseDto"/></response>
    /// <response code="404">Order not found</response>
    /// <response code="400">Invalid cancel request</response>
    /// <response code="500">Internal server error during order cancellation</response>
    [HttpDelete("{id}/cancel")]
    [ProducesResponseType(typeof(OrderMutationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequestDto? request)
    {
        var result = await _orderOrchestration.CancelOrderAsync(id, request?.Reason);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    ///  Update the status of an existing order
    /// </summary>
    /// <param name="id"> the order id to update </param>
    /// <param name="requestDto"> change status request</param>
    /// <returns>the update order id  </returns>
    /// <response code="200">Order status updated <see cref="OrderMutationResponseDto"/></response>
    /// <response code="404">Order not found</response>
    /// <response code="400">Invalid status update request</response>
    /// <response code="500">Internal server error during order status update</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(OrderMutationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequestDto requestDto)
    {
        var result = await _orderOrchestration.UpdateOrderStatusAsync(id, requestDto.Status);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    ///  Add an item to an existing order
    /// </summary>
    /// <param name="orderId"> the order id to update</param>
    /// <param name="request"> new item  information <see cref="OrderItemDto"/> </param>
    /// <returns> Orderid with new created itemid  </returns>
    /// <response code="200">Item added to order <see cref="AddOrderItemResultDto"/></response>
    /// <response code="404">Order not found</response>
    /// <response code="400">Invalid item data</response>
    /// <response code="500">Internal server error during adding item to order</response>
    [HttpPost("{orderId}/items")]
    [ProducesResponseType(typeof(AddOrderItemResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddOrderItem(string orderId, [FromBody] OrderItemDto request)
    {
        _logger.LogInformation("Adding item to order {OrderId}", orderId);

        var result = await _orderOrchestration.AddOrderItemAsync(orderId, request);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

 
    /// <summary>
    ///  Remove an item from an existing order
    /// </summary>
    /// <param name="orderId">the order id to update </param>
    /// <param name="itemId"> the item id to remove </param>
    /// <returns></returns>
    [HttpDelete("{orderId}/items/{itemId}")]
    [ProducesResponseType(typeof(RemoveOrderItemResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveOrderItem(string orderId, string itemId)
    {
        _logger.LogInformation("Removing item {ItemId} from order {OrderId}", itemId, orderId);

        var result = await _orderOrchestration.RemoveOrderItem(orderId, itemId);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    ///  Update the quantity of an item in an existing order
    /// </summary>
    /// <param name="orderId">the order id to update </param>
    /// <param name="itemId"> the item id to update </param>
    /// <param name="requestDto"> the new quantity </param>
    /// <returns> the updated item information </returns>
    /// <response code="200">Item quantity updated <see cref="UpdateOrderItemQuantityResultDto"/></response>
    /// <response code="404">Order or item not found</response>
    /// <response code="400">Invalid quantity update request</response>
    /// <response code="500">Internal server error during item quantity update</response>
    [HttpPatch("{orderId}/items/{itemId}/quantity")]
    [ProducesResponseType(typeof(UpdateOrderItemQuantityResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrderItemQuantity(string orderId, string itemId, [FromBody] UpdateItemQuantityRequestDto requestDto)
    {
        _logger.LogInformation("Updating item {ItemId} quantity in order {OrderId} to {NewQuantity}",
            itemId, orderId, requestDto.NewQuantity);

        var result = await _orderOrchestration.UpdateOrderItemQuantity(orderId, itemId, requestDto.NewQuantity);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

 
}



