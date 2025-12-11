namespace ServiceCatalogService.Api.Services.Interfaces;

public interface IUserContextService
{
    Guid? GetTenantId();
    bool IsCustomer();
    void ValidateProviderAccess(string resource);
}