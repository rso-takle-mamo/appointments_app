using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Api.Models;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register/customer")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequest request)
    {
        var response = await authService.RegisterCustomerAsync(request);
        return Ok(response);
    }
    
    [HttpPost("register/provider")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> RegisterProvider([FromBody] ProviderRegisterRequest request)
    {
        var response = await authService.RegisterProviderAsync(request);
        return Ok(response);
    }
    
    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }
    
    [HttpPost("logout")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        await authService.LogoutAsync(authorizationHeader);
        return Ok(new { message = "Logout successful" });
    }
}