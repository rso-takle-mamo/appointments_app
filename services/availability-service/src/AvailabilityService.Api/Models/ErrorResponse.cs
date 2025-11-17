namespace AvailabilityService.Api.Models;

public class ErrorResponse
{
    public Error Error { get; set; } = new Error();
}

public class Error
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public List<ErrorDetail> Details { get; set; } = new List<ErrorDetail>();
    public string? TraceId { get; set; }
}

public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}