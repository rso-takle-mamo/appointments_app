using System.ComponentModel.DataAnnotations;

namespace ServiceCatalogService.Api.Requests;

public class ServiceFilterRequest
{
    // Tenant filtering for Customers (optional)
    public Guid? TenantId { get; set; }

    [StringLength(100, ErrorMessage = "Tenant name cannot exceed 100 characters")]
    public string? TenantName { get; set; }

    // Price filtering
    [Range(0, 9999999999.99, ErrorMessage = "Minimum price must be between 0 and 9,999,999,999.99")]
    public decimal? MinPrice { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "Maximum price must be between 0 and 9,999,999,999.99")]
    public decimal? MaxPrice { get; set; }

    // Duration filtering (in minutes)
    [Range(1, 480, ErrorMessage = "Maximum duration must be between 1 and 480 minutes")]
    public int? MaxDuration { get; set; }

    // Category filtering
    public Guid? CategoryId { get; set; }

    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string? CategoryName { get; set; }

    // Activity status filtering
    public bool? IsActive { get; set; }

    // Pagination parameters
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be greater than or equal to 0")]
    public int Offset { get; set; } = 0;

    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; set; } = 100;
}