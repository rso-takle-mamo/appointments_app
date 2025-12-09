using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Services;
using ServiceCatalogService.Api.Services.Interfaces;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceService serviceService, IUserContextService userContextService) : ControllerBase
{
    /// <summary>
    /// Get services with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **CUSTOMERS:**
    /// - Access to services from ALL tenants
    ///
    /// **PROVIDERS:**
    /// - Access ONLY to services from their own tenant
    /// - Setting tenantId in query is NOT allowed
    /// </remarks>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PaginatedResponse<ServiceResponse>>> GetServices([FromQuery] ServiceFilterRequest request)
    {
        var isCustomer = userContextService.IsCustomer();
        Guid? userTenantId = isCustomer ? null : userContextService.GetTenantId();

        var (services, totalCount) = await serviceService.GetServicesAsync(request, isCustomer, userTenantId);

        var response = new PaginatedResponse<ServiceResponse>
        {
            Offset = request.Offset,
            Limit = request.Limit,
            TotalCount = totalCount,
            Data = services.ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Get service by id
    /// </summary>
    /// <remarks>
    /// **CUSTOMERS:**
    /// - Access to services from ALL tenants
    ///
    /// **PROVIDERS:**
    /// - Access ONLY to services from their own tenant
    /// </remarks>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ServiceResponse>> GetService(Guid id)
    {
        var isCustomer = userContextService.IsCustomer();
        Guid? userTenantId = isCustomer ? null : userContextService.GetTenantId();

        var service = await serviceService.GetServiceByIdAsync(id, isCustomer, userTenantId);

        return Ok(service);
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    /// <remarks>
    /// **Providers only**: Creates a new service within the provider's tenant.
    /// </remarks>
    /// <param name="request">Service creation details with all service properties</param>
    /// <response code="201">Service successfully created</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ServiceResponse>> CreateService([FromBody] CreateServiceRequest request)
    {
        userContextService.ValidateProviderAccess();
        var tenantId = userContextService.GetTenantId();

        var response = await serviceService.CreateServiceAsync(request, tenantId);

        return CreatedAtAction(nameof(GetService), new { id = response.Id }, response);
    }

    /// <summary>
    /// Update service
    /// </summary>
    /// <remarks>
    /// **Providers only**: Updates a service within the provider's tenant.
    /// </remarks>
    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ServiceResponse>> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
    {
        userContextService.ValidateProviderAccess();
        var userTenantId = userContextService.GetTenantId();

        var response = await serviceService.UpdateServiceAsync(id, request, userTenantId);

        return Ok(response);
    }

    /// <summary>
    /// Delete service.
    /// </summary>
    ///  <remarks>
    /// **Providers only**: Deletes a service within the provider's tenant.
    /// </remarks>
    /// <response code="204">Service successfully deleted</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        userContextService.ValidateProviderAccess();
        var userTenantId = userContextService.GetTenantId();

        await serviceService.DeleteServiceAsync(id, userTenantId);

        return NoContent();
    }
}