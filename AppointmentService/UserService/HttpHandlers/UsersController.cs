using Microsoft.AspNetCore.Mvc;
using UserService.Extensions;
using UserService.Model;
using UserService.Requests;
using UserService.Services;

namespace UserService.HttpHandlers;

[ApiController]
[Route("api/users")]
public class UsersController(UserRepository userRepository) : Controller
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok((await userRepository.Get(id)).ToDto());
    }

    [HttpPost]
    [ServiceFilter<ValidModelFilter>]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateUser createUser)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUser.FirstName,
            LastName = createUser.LastName,
            Email = createUser.Email,
            UserType = UserType.Customer,
            Username = createUser.Username,
            Password = createUser.Password,
        };
        
        await userRepository.Create(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user.ToDto());
    }
}