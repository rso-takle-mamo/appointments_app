namespace UserService.Api.Models;

public class Error
{
    public required string Code { get; set; }
    public required string Message { get; set; }
}