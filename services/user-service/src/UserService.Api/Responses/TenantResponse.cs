using System.Text.Json.Serialization;
using UserService.Database.Entities;

namespace UserService.Api.Responses;

public class TenantResponse
{
    /// <summary>
    /// The unique identifier for the tenant
    /// </summary>
    /// <example>456e7890-e89b-12d3-a456-426614174001</example>
    public required Guid Id { get; set; }

    /// <summary>
    /// The user ID of the tenant owner
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public required Guid OwnerId { get; set; }

    /// <summary>
    /// The VAT number of the tenant
    /// </summary>
    /// <example>DE123456789</example>
    public required string VatNumber { get; set; }

    /// <summary>
    /// The business name of the tenant
    /// </summary>
    /// <example>Jane's Professional Services</example>
    public required string BusinessName { get; set; }

    /// <summary>
    /// The business email address
    /// </summary>
    /// <example>contact@business.com</example>
    public string? BusinessEmail { get; set; }

    /// <summary>
    /// The business phone number
    /// </summary>
    /// <example>+1-555-123-4567</example>
    public string? BusinessPhone { get; set; }

    /// <summary>
    /// The business address
    /// </summary>
    /// <example>123 Business St, City, State 12345</example>
    public string? Address { get; set; }

    /// <summary>
    /// The business description
    /// </summary>
    /// <example>Professional consulting and advisory services</example>
    public string? Description { get; set; }

    /// <summary>
    /// The timestamp when the tenant was created
    /// </summary>
    /// <example>2025-12-09T10:00:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The timestamp when the tenant was last updated
    /// </summary>
    /// <example>2025-12-09T11:00:00Z</example>
    public DateTime UpdatedAt { get; set; }
}

public static class TenantResponseExtensions
{
    public static TenantResponse ToResponse(this Tenant tenant)
    {
        return new TenantResponse
        {
            Id = tenant.Id,
            OwnerId = tenant.OwnerId,
            VatNumber = tenant.VatNumber,
            BusinessName = tenant.BusinessName,
            BusinessEmail = tenant.BusinessEmail,
            BusinessPhone = tenant.BusinessPhone,
            Address = tenant.Address,
            Description = tenant.Description,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }
}