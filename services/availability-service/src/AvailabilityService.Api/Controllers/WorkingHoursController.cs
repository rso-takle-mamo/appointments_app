using Microsoft.AspNetCore.Mvc;
using AvailabilityService.Api.Models;
using AvailabilityService.Database.Models;

namespace AvailabilityService.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class WorkingHoursController : ControllerBase
{
    private readonly ILogger<WorkingHoursController> _logger;

    public WorkingHoursController(ILogger<WorkingHoursController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all working hours
    /// </summary>
    /// <returns>A successful response with working hours data</returns>
    [HttpGet("working-hours")]
    public async Task<IActionResult> GetWorkingHours()
    {
        _logger.LogInformation("Getting all working hours");

        // TODO: Implement actual logic to retrieve working hours from repository
        // For now, return a stub response

        var response = new
        {
            Message = "Working hours endpoint is working",
            Timestamp = DateTime.UtcNow,
            Data = new object[] { }
        };

        return Ok(response);
    }

    /// <summary>
    /// Get working hours by day of week
    /// </summary>
    /// <param name="dayOfWeek">Day of the week (0-6, where 0 = Sunday)</param>
    /// <returns>A successful response with working hours for the specified day</returns>
    [HttpGet("working-hours/{dayOfWeek:int}")]
    public async Task<IActionResult> GetWorkingHoursByDay(int dayOfWeek)
    {
        _logger.LogInformation("Getting working hours for day: {Day}", dayOfWeek);

        if (dayOfWeek < 0 || dayOfWeek > 6)
        {
            return BadRequest(new ErrorResponse
            {
                Error = new Error
                {
                    Code = "INVALID_DAY",
                    Message = "Day of week must be between 0 (Sunday) and 6 (Saturday)",
                    TraceId = HttpContext.TraceIdentifier
                }
            });
        }

        var response = new
        {
            Message = $"Working hours for day {dayOfWeek} endpoint is working",
            DayOfWeek = dayOfWeek,
            Timestamp = DateTime.UtcNow,
            Data = new object[] { }
        };

        return Ok(response);
    }
}