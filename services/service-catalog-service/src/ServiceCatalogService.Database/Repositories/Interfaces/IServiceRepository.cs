using ServiceCatalogService.Database.Entities;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Database.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<(IReadOnlyCollection<Service> Services, int TotalCount)> GetServicesAsync(PaginationParameters parameters, ServiceFilterParameters filters);
    Task<Service?> GetServiceByIdAsync(Guid id);
    Task CreateServiceAsync(Service service);
    Task<bool> UpdateServiceAsync(Guid id, UpdateService updateRequest);
    Task<bool> DeleteServiceAsync(Guid id);
}

public class ServiceFilterParameters
{
    public Guid? TenantId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MaxDuration { get; set; }
    public string? ServiceName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? IsActive { get; set; }
    public string? Address { get; set; }
    public string? BusinessName { get; set; }
}