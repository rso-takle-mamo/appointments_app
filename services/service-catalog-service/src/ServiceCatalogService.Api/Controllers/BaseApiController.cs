using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Services.Interfaces;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController(IUserContextService userContextService) : ControllerBase
{
    protected Guid? GetTenantId() => userContextService.GetTenantId();
    protected bool IsCustomer() => userContextService.IsCustomer();
    protected void ValidateProviderAccess(string resource) => userContextService.ValidateProviderAccess(resource);
}