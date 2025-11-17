using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        await authService.LogoutAsync(authorizationHeader);
        return Ok(new { message = "Logout successful" });
    }
}