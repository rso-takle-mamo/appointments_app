using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Services;
using UserService.Api.Filters;
using UserService.Api.Exceptions;
using UserService.Api.Models;

namespace UserService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromToken();
        var profile = await userService.GetProfileAsync(userId);
        return Ok(profile);
    }
    
    [HttpPatch("me")]
    [ServiceFilter(typeof(ModelValidationFilter))]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserIdFromToken();
        var updatedProfile = await userService.UpdateProfileAsync(userId, request);
        return Ok(updatedProfile);
    }
    
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = GetUserIdFromToken();
        await userService.DeleteUserAsync(userId);
        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new AuthenticationException("token", "Invalid user token");
        }

        return userId;
    }
}