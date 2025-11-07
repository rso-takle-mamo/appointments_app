using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Interfaces;
using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;
using ServiceCatalogService.Api.Services;

namespace ServiceCatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IServiceService serviceService) : ControllerBase
{
    private readonly IServiceService _serviceService = serviceService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetServices([FromQuery] Guid? tenantId = null)
    {
        var services = await _serviceService.GetAllServicesAsync(tenantId);
        var response = services.Where(s => s.IsActive).Select(s => s.ToServiceResponse());
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse>> GetService(Guid id)
    {
        var service = await _serviceService.GetServiceByIdAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        return Ok(service.ToServiceResponse());
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetServicesByTenant(Guid tenantId)
    {
        var services = await _serviceService.GetAllServicesAsync(tenantId);
        var response = services.Where(s => s.IsActive).Select(s => s.ToServiceResponse());
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> CreateService([FromBody] CreateServiceRequest request)
    {
        var service = await _serviceService.CreateServiceAsync(request);
        var response = service.ToServiceResponse();
        return CreatedAtAction(nameof(GetService), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceRequest request)
    {
        var success = await _serviceService.UpdateServiceAsync(id, request);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var success = await _serviceService.DeleteServiceAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleService(Guid id)
    {
        var success = await _serviceService.ToggleServiceAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}