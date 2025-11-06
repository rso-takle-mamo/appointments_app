using Microsoft.AspNetCore.Mvc;
using UserService.Api.Data;
using UserService.Api.Extensions;
using UserService.Api.Models.Dtos;
using UserService.Api.Models.Entities;
using UserService.Api.Services;
using UserService.Api.Validators;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UserRepository userRepository) : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateUserRequest createUserRequest)
    {
        if (await userRepository.UsernameExists(createUserRequest.Username))
        {
            return BadRequest(new { Message = "Username already exists" });
        }

        var roleString = createUserRequest.Role ??
            (createUserRequest.TenantId.HasValue ? "Provider" : "Customer");
        var userRole = UserRoleExtensions.FromPostgresValue(roleString);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUserRequest.FirstName,
            LastName = createUserRequest.LastName,
            Username = createUserRequest.Username,
            Password = createUserRequest.Password,
            Role = userRole,
            TenantId = createUserRequest.TenantId
        };

        await userRepository.Create(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user.ToResponse());
    }
}