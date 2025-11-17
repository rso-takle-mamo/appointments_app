using UserService.Api.Requests;
using UserService.Api.Responses;

namespace UserService.Api.Services;

public interface ITenantService
{
    Task<TenantResponse> GetTenantAsync(Guid tenantId, Guid userId);
    Task<TenantResponse> CreateTenantAsync(Guid userId, CreateTenantRequest request);
    Task<TenantResponse> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request);
    Task DeleteTenantAsync(Guid tenantId, Guid userId);
}