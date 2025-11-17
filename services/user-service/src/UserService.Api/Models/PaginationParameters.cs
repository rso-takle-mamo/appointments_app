using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Models;

public class PaginationParameters
{
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int Offset { get; set; } = 0;

    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; set; } = 100;
}