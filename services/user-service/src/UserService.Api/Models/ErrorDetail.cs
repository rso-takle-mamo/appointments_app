namespace UserService.Api.Models;

public class ErrorDetail
{
    public required string Code { get; set; }
    public required string Message { get; set; }
}