using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Extensions;
using UserService.Database.Entities;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Api.Services;

public class UserService(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    ISessionService sessionService
) : IUserService
{
    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Only update fields that are provided and changed
        bool hasUpdates = false;

        // Validate username uniqueness if it's being updated
        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            if (await userRepository.UsernameExistsExcept(userId, request.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }
            user.Username = request.Username;
            hasUpdates = true;
        }

        // Validate tenant exists if TenantId is provided
        if (request.TenantId.HasValue && request.TenantId.Value != user.TenantId)
        {
            if (request.TenantId.Value != Guid.Empty && !await tenantRepository.Exists(request.TenantId.Value))
            {
                throw new InvalidOperationException("Tenant does not exist");
            }
            user.TenantId = request.TenantId.Value;
            hasUpdates = true;
        }

        // Update other fields if provided
        if (!string.IsNullOrEmpty(request.FirstName) && request.FirstName != user.FirstName)
        {
            user.FirstName = request.FirstName;
            hasUpdates = true;
        }

        if (!string.IsNullOrEmpty(request.LastName) && request.LastName != user.LastName)
        {
            user.LastName = request.LastName;
            hasUpdates = true;
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            user.Email = request.Email;
            hasUpdates = true;
        }

        // Only update if there are actual changes
        if (hasUpdates)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var updatedUser = await userRepository.UpdateAsync(user);
            if (updatedUser == null)
            {
                throw new InvalidOperationException("Failed to update user profile");
            }
            return updatedUser.ToResponse();
        }

        // No changes made, return existing user
        return user.ToResponse();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Invalidate all user sessions before deletion
        await sessionService.InvalidateUserSessionsAsync(userId);

        // Delete the user
        var deleted = await userRepository.DeleteAsync(userId);
        if (!deleted)
        {
            throw new InvalidOperationException("Failed to delete user account");
        }
    }
}