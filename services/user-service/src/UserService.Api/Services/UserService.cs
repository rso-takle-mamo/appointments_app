using UserService.Api.Requests;
using UserService.Api.Responses;
using UserService.Api.Extensions;
using UserService.Api.Exceptions;
using UserService.Database.Entities;
using UserService.Database.Enums;
using UserService.Database.Repositories.Interfaces;
using UserService.Database;

namespace UserService.Api.Services;

public class UserService(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    ISessionService sessionService,
    UserDbContext dbContext
) : IUserService
{
    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.Get(userId);
        return user == null ? throw new NotFoundException("User", userId) : user.ToResponse();
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var hasUpdates = false;
        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            if (await userRepository.UsernameExistsExcept(userId, request.Username))
            {
                throw new ConflictException("username", "Username already exists");
            }
            user.Username = request.Username;
            hasUpdates = true;
        }

        // Note: TenantId cannot be updated through profile updates
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

        if (!hasUpdates) return user.ToResponse();
        user.UpdatedAt = DateTime.UtcNow;
        var updatedUser = await userRepository.UpdateAsync(user);
        
        return updatedUser == null ? throw new DatabaseOperationException("update", "User", "Failed to update user profile") : updatedUser.ToResponse();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.Get(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // Invalidate all user sessions
            await sessionService.InvalidateUserSessionsAsync(userId);

            // Delete tenant if user is a provider
            if (user is { Role: UserRole.Provider, TenantId: not null })
            {
                var tenantDeleted = await tenantRepository.DeleteAsync(user.TenantId.Value);
                if (!tenantDeleted)
                {
                    throw new DatabaseOperationException("delete", "Tenant", "Failed to delete tenant for provider");
                }
            }

            // Delete the user
            var deleted = await userRepository.DeleteAsync(userId);
            if (!deleted)
            {
                throw new DatabaseOperationException("delete", "User", "Failed to delete user account");
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            // Rollback transaction if any step fails
            await transaction.RollbackAsync();
            throw;
        }
    }
}