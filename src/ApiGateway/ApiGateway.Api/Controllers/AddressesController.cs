using ApiGateway.Core.Address;
using ApiGateway.Core.Address.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;


[ApiController]
[Route("api/addresses")]
[Produces("application/json")]
public class AddressesController : Base
{
    private readonly IAddressServiceClient _addressServiceClient;
    private readonly ILogger<AddressesController> _logger;
    public AddressesController(
        IAddressServiceClient addressServiceClient,
        ILogger<AddressesController> logger) : base(logger)
    {
        _addressServiceClient = addressServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a single address by its ID
    /// </summary>
    /// <param name="id">The address ID (GUID format)</param>
    /// <returns>The address details</returns>
    /// <response code="200">Returns the address details</response>
    /// <response code="404">Address not found</response>
    /// <response code="400">Invalid address ID format</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddress(string id,CancellationToken cancellationToken)
    {
        var result = await _addressServiceClient.GetAddressById(id,cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Retrieves all addresses for a specific user
    /// </summary>
    /// <param name="userId">The user ID (GUID format)</param>
    /// <returns>List of addresses</returns>
    /// <response code="200">Returns the list of addresses</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddressesByUser(string userId)
    {
        var result = await _addressServiceClient.GetAddressesByUser(userId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Retrieves the default address for a specific user
    /// </summary>
    /// <param name="userId">The user ID (GUID format)</param>
    /// <returns>The default address</returns>
    /// <response code="200">Returns the default address</response>
    /// <response code="404">Default address not found</response>
    [HttpGet("user/{userId}/default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDefaultAddress(string userId)
    {
        var result = await _addressServiceClient.GetDefaultAddress(userId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Retrieves a paginated list of addresses for a specific user
    /// </summary>
    /// <param name="userId">The user ID (GUID format)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>A paginated list of addresses</returns>
    /// <response code="200">Returns the paginated address list</response>
    [HttpGet("user/{userId}/paged/{pageNumber}/{pageSize}")]
    [ProducesResponseType(typeof(PagedAddressesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPagedAddressesByUser(
        string userId,
        [FromRoute] int pageNumber = 1,
        [FromRoute] int pageSize = 10)
    {
        var result = await _addressServiceClient.GetPagedAddressesByUser(userId, pageNumber, pageSize);

        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Creates a new address
    /// </summary>
    /// <param name="request">The address creation data</param>
    /// <returns>The created address</returns>
    /// <response code="201">Address created successfully</response>
    /// <response code="400">Invalid address data</response>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequestDto request)
    {
        var result = await _addressServiceClient.CreateAddress(request);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetAddress), new { id = result.Data.AddressId }, result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Updates an existing address
    /// </summary>
    /// <param name="id">The address ID to update</param>
    /// <param name="request">The address update data (only provide fields to update)</param>
    /// <returns>The updated address</returns>
    /// <response code="200">Address updated successfully</response>
    /// <response code="404">Address not found</response>
    /// <response code="400">Invalid address data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAddress(string id, [FromBody] UpdateAddressRequestDto request,CancellationToken cancellationToken)
    {
        var result = await _addressServiceClient.UpdateAddress(id,request,cancellationToken);

        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Deletes an address
    /// </summary>
    /// <param name="id">The address ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Address deleted successfully</response>
    /// <response code="404">Address not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        var result = await _addressServiceClient.DeleteAddress(id);

        if (result.IsSuccess)
            return NoContent();

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Sets an address as the default for the user
    /// </summary>
    /// <param name="id">The address ID to set as default</param>
    /// <returns>The updated address</returns>
    /// <response code="200">Address set as default successfully</response>
    /// <response code="404">Address not found</response>
    [HttpPatch("{id}/set-default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetDefaultAddress(string id)
    {
        var result = await _addressServiceClient.SetDefaultAddress(id);

        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }

  
}
