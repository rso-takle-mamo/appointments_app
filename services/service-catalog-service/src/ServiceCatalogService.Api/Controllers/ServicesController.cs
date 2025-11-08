using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Dtos;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IServiceRepository serviceRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetServices([FromQuery] Guid? tenantId = null)
    {
        var services = await serviceRepository.GetAllServicesAsync(tenantId);
        var response = services.Where(s => s.IsActive).Select(s => s.ToServiceResponse());
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

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetServicesByTenant(Guid tenantId)
    {
        var services = await serviceRepository.GetAllServicesAsync(tenantId);
        var response = services.Where(s => s.IsActive).Select(s => s.ToServiceResponse());
        return Ok(response);
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
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

        return NoContent();
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

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> ToggleService(Guid id)
    {
        var success = await serviceRepository.ToggleServiceAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}