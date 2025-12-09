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
    /// <summary>
    /// Register a new customer account
    /// </summary>
    /// <param name="request">The customer registration details</param>
    /// <returns>JWT token for authentication</returns>
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterRequest request)
    {
        var response = await authService.RegisterCustomerAsync(request);
        return Ok(response);
    }
  
    /// <summary>
    /// Register a new provider account with tenant information
    /// </summary>
    /// <param name="request">The provider registration details including business information</param>
    /// <returns>JWT token for authentication</returns>
    [HttpPost("register/provider")]
    public async Task<IActionResult> RegisterProvider([FromBody] ProviderRegisterRequest request)
    {
        var response = await authService.RegisterProviderAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <returns>JWT token for authentication</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Logout user and invalidate the current session
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        await authService.LogoutAsync(authorizationHeader);
        return Ok(new { message = "Logout successful" });
    }
}