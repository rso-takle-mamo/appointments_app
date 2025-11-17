using System.Text.Json.Serialization;
using UserService.Database.Entities;

namespace UserService.Api.Responses;

public class TenantResponse
{
    public required Guid Id { get; set; }

    public required Guid OwnerId { get; set; }

    public required string BusinessName { get; set; }

    public string? BusinessEmail { get; set; }

    public string? BusinessPhone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

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