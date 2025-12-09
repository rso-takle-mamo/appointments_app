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
public class UsersController(IUserService userService) : BaseApiController
{
    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserIdFromToken();
        var profile = await userService.GetProfileAsync(userId);
        return Ok(profile);
    }
    
    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="request">User profile update information</param>
    /// <returns>Updated user profile</returns>
    [HttpPatch("me")]
    [ServiceFilter(typeof(ModelValidationFilter))]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserIdFromToken();
        var updatedProfile = await userService.UpdateProfileAsync(userId, request);
        return Ok(updatedProfile);
    }
    
    /// <summary>
    /// Delete current user account
    /// </summary>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = GetUserIdFromToken();
        await userService.DeleteUserAsync(userId);
        return NoContent();
    }
}