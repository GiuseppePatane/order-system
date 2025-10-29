using ApiGateway.Core.Common;
using ApiGateway.Core.User;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

/// <summary>
/// Manages user operations through the User gRPC service
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserServiceClient userServiceClient,
        ILogger<UsersController> logger)
    {
        _userServiceClient = userServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a single user by their ID
    /// </summary>
    /// <param name="id">The user ID (GUID format)</param>
    /// <returns>The user details</returns>
    /// <response code="200">Returns the user details</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user ID format</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _userServiceClient.GetUserById(id);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }


        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">The user creation data</param>
    /// <returns>The created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid user data or duplicate email</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        var result = await _userServiceClient.CreateUser(request);
        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(nameof(GetUser), new { id = result.Data.UserId }, result.Data);
        }

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    /// Retrieves a paginated list of users
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Optional search term for user name or email</param>
    /// <returns>A paginated list of users</returns>
    /// <response code="200">Returns the paginated user list</response>
    [HttpGet("{pageNumber}/{pageSize}")]
    [ProducesResponseType(typeof(PagedUsersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromRoute] int pageNumber = 1, [FromRoute] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var request = new GetUsersRequestDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _userServiceClient.GetUsers(request);
        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }


    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="request">The user update data (only provide fields to update)</param>
    /// <returns>The updated user</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="400">Invalid user data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto request)
    {
        request = request with { UserId = id };
        var result = await _userServiceClient.UpdateUser(request);
        if (result.IsSuccess && result.Data != null)
            return Ok(result.Data);

        return MapErrorToProblemDetails(result.Error!);
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">User deleted successfully</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userServiceClient.DeleteUser(id);
        if (result.IsSuccess && result.Data != null && result.Data.Success)
            return NoContent();

        return MapErrorToProblemDetails(result.Error!);
    }

    private IActionResult MapErrorToProblemDetails(ErrorInfo error)
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
            "USER_NOT_FOUND" => (StatusCodes.Status404NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),
            "INVALID_USER_ID" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "INVALID_ARGUMENT" => (StatusCodes.Status400BadRequest, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            "GRPC_ERROR" => (StatusCodes.Status503ServiceUnavailable, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4"),
            "EMPTY_RESPONSE" => (StatusCodes.Status502BadGateway, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3"),
            _ => (StatusCodes.Status500InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };

        problemDetails.Status = statusCode;
        problemDetails.Type = type;

        _logger.LogWarning("Returning error response: {Code} with HTTP status {StatusCode}", error.Code, statusCode);

        return StatusCode(statusCode, problemDetails);
    }
}
