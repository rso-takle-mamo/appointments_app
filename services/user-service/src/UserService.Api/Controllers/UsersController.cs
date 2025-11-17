using Microsoft.AspNetCore.Mvc;
using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Extensions;
using UserService.Api.Validators;
using UserService.Database.Entities;
using UserService.Database.Enums;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserRepository userRepository, ITenantRepository tenantRepository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await userRepository.Get(id);
        if (user == null) return NotFound();
        return Ok(user.ToResponse());
    }

    [HttpPost]
    [ServiceFilter(typeof(ValidModelFilter))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post([FromBody] CreateUserRequest createUserRequest)
    {
        if (await userRepository.UsernameExists(createUserRequest.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Validate tenant if TenantId is provided
        if (createUserRequest.TenantId.HasValue && !await tenantRepository.Exists(createUserRequest.TenantId.Value))
        {
            throw new InvalidOperationException("Tenant does not exist");
        }

        // Determine role based on tenant assignment
        var role = createUserRequest.TenantId.HasValue ? UserRole.Provider : UserRole.Customer;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUserRequest.FirstName,
            LastName = createUserRequest.LastName,
            Username = createUserRequest.Username,
            Password = createUserRequest.Password,
            Email = createUserRequest.Email,
            Role = role,
            TenantId = createUserRequest.TenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.Create(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user.ToResponse());
    }
}