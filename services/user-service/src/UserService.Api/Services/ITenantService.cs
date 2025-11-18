using UserService.Api.Requests;
using UserService.Api.Responses;

namespace UserService.Api.Services;

public interface ITenantService
{
    Task<TenantResponse> GetTenantAsync(Guid tenantId, Guid userId);
    Task<TenantResponse> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request);
}