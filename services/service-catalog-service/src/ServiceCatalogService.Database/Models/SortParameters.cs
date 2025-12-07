using ServiceCatalogService.Database.Enums;

namespace ServiceCatalogService.Database.Models;

public class SortParameters
{
    public ServiceSortField Field { get; set; } = ServiceSortField.Name;
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}