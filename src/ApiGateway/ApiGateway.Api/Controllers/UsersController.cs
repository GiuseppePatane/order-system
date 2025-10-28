using ApiGateway.Core.Common;
using ApiGateway.Core.User;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("api/users")]
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


    [HttpGet("{page}/{pageSize}")]
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
