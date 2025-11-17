namespace UserService.Api.Models;

public class PaginatedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int Total { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}