using UserService.Api.Requests;
using UserService.Api.Responses;

namespace UserService.Api.Services;

public interface IUserService
{
    Task<UserResponse> GetProfileAsync(Guid userId);
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request);
    Task DeleteUserAsync(Guid userId);
}