using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ServiceCatalogService.Api.Models.DTOs;

public class CreateServiceRequest
{
    [Required(ErrorMessage = "Tenant ID is required")]
    public Guid TenantId { get; set; }

    [Required(ErrorMessage = "Service name is required")]
    [MaxLength(255, ErrorMessage = "Service name cannot exceed 255 characters")]
    public string Name { get; set; } = null!;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99")]
    [Precision(2)]
    public decimal Price { get; set; }

    [Range(5, 480, ErrorMessage = "Duration must be between 5 and 480 minutes")]
    public int DurationMinutes { get; set; }

    public Guid? CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}