namespace AvailabilityService.Api.Responses;

public class AvailableTimeRangeResponse
{
    public List<AvailableTimeRange> AvailableRanges { get; set; } = new();
}

public class AvailableTimeRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}