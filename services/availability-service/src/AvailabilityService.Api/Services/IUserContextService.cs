namespace AvailabilityService.Api.Services;

public interface IUserContextService
{
    Guid GetTenantId();
    string GetRole();
    bool IsCustomer();
    void ValidateTenantAccess(Guid tenantId, string resource);
    void ValidateProviderAccess();
}