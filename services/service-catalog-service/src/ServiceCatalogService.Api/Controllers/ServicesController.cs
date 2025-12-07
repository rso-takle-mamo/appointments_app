using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Services;
using ServiceCatalogService.Api.Exceptions;
using ServiceCatalogService.Database.Enums;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceRepository serviceRepository, IUserContextService userContextService) : ControllerBase
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
        // Customer-only filters validation
        if (!userContextService.IsCustomer())
        {
            if (!string.IsNullOrEmpty(request.Address) || !string.IsNullOrEmpty(request.BusinessName))
            {
                throw new AuthorizationException("Service", "filter", "Address and BusinessName filters are available only to customers.");
            }
        }

        if (userContextService.IsCustomer())
        {
            var parameters = new PaginationParameters
            {
                Offset = request.Offset,
                Limit = request.Limit,
                Sort = new SortParameters
                {
                    Field = request.OrderBy ?? ServiceSortField.Name,
                    Direction = request.OrderDirection ?? SortDirection.Ascending
                }
            };

            var filters = new ServiceFilterParameters
            {
                TenantId = request.TenantId,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MaxDuration = request.MaxDuration,
                ServiceName = request.ServiceName,
                CategoryId = request.CategoryId,
                CategoryName = request.CategoryName,
                IsActive = request.IsActive,
                Address = request.Address,
                BusinessName = request.BusinessName
            };

            var (services, totalCount) = await serviceRepository.GetServicesAsync(parameters, filters);

            var response = new PaginatedResponse<ServiceResponse>
            {
                Offset = parameters.Offset,
                Limit = parameters.Limit,
                TotalCount = totalCount,
                Data = services.Select(s => s.ToServiceResponse()).ToList()
            };

            return Ok(response);
        }
        // provider
        else
        {
            // providers should not attempt to specify tenantId in query parameters
            if (request.TenantId.HasValue)
            {
                throw new AuthorizationException("Service", "filter", "Providers cannot specify tenantId parameter. Tenant access is automatically enforced from authentication token.");
            }

            var parameters = new PaginationParameters
            {
                Offset = request.Offset,
                Limit = request.Limit,
                Sort = new SortParameters
                {
                    Field = request.OrderBy ?? ServiceSortField.Name,
                    Direction = request.OrderDirection ?? SortDirection.Ascending
                }
            };

            var tenantId = userContextService.GetTenantId();
            var filters = new ServiceFilterParameters
            {
                TenantId = tenantId,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MaxDuration = request.MaxDuration,
                ServiceName = request.ServiceName,
                CategoryId = request.CategoryId,
                CategoryName = request.CategoryName,
                IsActive = request.IsActive,
                Address = null, // Providers cannot use tenant-based filters
                BusinessName = null // Providers cannot use tenant-based filters
            };

            var (services, totalCount) = await serviceRepository.GetServicesAsync(parameters, filters);

            var response = new PaginatedResponse<ServiceResponse>
            {
                Offset = parameters.Offset,
                Limit = parameters.Limit,
                TotalCount = totalCount,
                Data = services.Select(s => s.ToServiceResponse()).ToList()
            };

            return Ok(response);
        }
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
    public async Task<ActionResult<object>> GetService(Guid id)
    {
        var service = await serviceRepository.GetServiceByIdAsync(id);

        if (service == null)
        {
            throw new NotFoundException("Service", id);
        }

        if (!userContextService.IsCustomer())
        {
            userContextService.ValidateTenantAccess(service.TenantId, "Service");
        }

        return Ok(service.ToServiceResponse());
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    /// <remarks>
    /// **Providers only**: Creates a new service within the provider's tenant.
    /// </remarks>
    /// <param name="request">Service creation details with all service properties</param>
    /// <returns>Created service information with location header</returns>
    /// <response code="201">Service successfully created</response>
    /// <response code="400">Invalid request data (negative price, invalid duration, etc.)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a Provider</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ServiceResponse>> CreateService([FromBody] CreateServiceRequest request)
    {
        userContextService.ValidateProviderAccess();
        var tenantId = userContextService.GetTenantId();
        var service = request.ToEntity(tenantId);
        service.Id = Guid.NewGuid();
        await serviceRepository.CreateServiceAsync(service);
        var response = service.ToServiceResponse();
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
        var existingService = await serviceRepository.GetServiceByIdAsync(id);

        if (existingService == null)
        {
            throw new NotFoundException("Service", id);
        }

        userContextService.ValidateTenantAccess(existingService.TenantId, "Service");

        var updateRequest = new UpdateService
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
        };

        var success = await serviceRepository.UpdateServiceAsync(id, updateRequest);

        if (!success)
        {
            throw new DatabaseOperationException("Update", "Service", "Failed to update service");
        }

        var updatedService = await serviceRepository.GetServiceByIdAsync(id);
        return Ok(updatedService!.ToServiceResponse());
    }

    /// <summary>
    /// Delete service.
    /// </summary>
    ///  <remarks>
    /// **Providers only**: Deletes a service within the provider's tenant.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        userContextService.ValidateProviderAccess();
        var existingService = await serviceRepository.GetServiceByIdAsync(id);

        if (existingService == null)
        {
            throw new NotFoundException("Service", id);
        }

        userContextService.ValidateTenantAccess(existingService.TenantId, "Service");
        var success = await serviceRepository.DeleteServiceAsync(id);

        return !success ? throw new DatabaseOperationException("Delete", "Service", "Failed to delete service") : NoContent();
    }
}