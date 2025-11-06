using UserService.Api.Data;
using UserService.Api.Models.Entities;
using UserService.Api.Models.Dtos;

namespace UserService.Api.Extensions;

public static class UserExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToPostgresValue(),
            TenantId = user.TenantId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}