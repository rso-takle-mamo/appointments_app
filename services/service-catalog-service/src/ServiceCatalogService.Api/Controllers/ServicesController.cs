using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceRepository serviceRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ServiceResponse>>> GetServices(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 100,
        [FromQuery] Guid? tenantId = null)
    {
        if (offset < 0)
        {
            throw new ArgumentException("Offset must be greater than or equal to 0");
        }

        if (limit is < 1 or > 100)
        {
            throw new ArgumentException("Limit must be between 1 and 100");
        }

        var parameters = new PaginationParameters
        {
            Offset = offset,
            Limit = limit
        };

        var (services, totalCount) = await serviceRepository.GetServicesAsync(parameters, tenantId);

        var response = new PaginatedResponse<ServiceResponse>
        {
            Offset = offset,
            Limit = limit,
            TotalCount = totalCount,
            Data = services.Where(s => s.IsActive).Select(s => s.ToServiceResponse()).ToList()
        };

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> GetService(Guid id)
    {
        var service = await serviceRepository.GetServiceByIdAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        return Ok(service.ToServiceResponse());
    }
    
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> CreateService([FromBody] CreateServiceRequest request)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
        };
        await serviceRepository.CreateServiceAsync(service);
        var response = service.ToServiceResponse();
        return CreatedAtAction(nameof(GetService), new { id = response.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
    {
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
            return NotFound();
        }

        // Get the updated service and return it
        var updatedService = await serviceRepository.GetServiceByIdAsync(id);
        return Ok(updatedService!.ToServiceResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var success = await serviceRepository.DeleteServiceAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    }