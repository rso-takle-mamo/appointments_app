using AvailabilityService.Api.Responses;
using AvailabilityService.Api.Services.Interfaces;
using AvailabilityService.Database.Entities;
using AvailabilityService.Database.Repositories.Interfaces;

namespace AvailabilityService.Api.Services;

public class AvailabilityService(
    IWorkingHoursRepository workingHoursRepository,
    ITimeBlockRepository timeBlockRepository,
    ITenantRepository tenantRepository,
    IBookingRepository bookingRepository)
    : IAvailabilityService
{
    public async Task<AvailableTimeRangeResponse> GetAvailableRangesAsync(Guid tenantId, DateTime startDate, DateTime endDate)
    {
        var tenant = await tenantRepository.GetTenantByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant not found: {tenantId}");
        }

        // Get all data for the date range
        var workingHours = await workingHoursRepository.GetWorkingHoursByTenantAndDateRangeAsync(tenantId, startDate, endDate);
        var timeBlocks = await timeBlockRepository.GetTimeBlocksByTenantAndDateRangeAsync(tenantId, startDate, endDate);
        var bookings = await bookingRepository.GetBookingsByTenantAndDateRangeAsync(tenantId, startDate, endDate);

        var response = new AvailableTimeRangeResponse();

        // Process each day in the range
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;

            // Get working hours for this day
            var dayWorkingHours = workingHours.FirstOrDefault(wh => wh.Day == dayOfWeek);

            // Skip if no working hours or if it's a day off (StartTime == EndTime)
            if (dayWorkingHours == null || IsDayFree(dayWorkingHours))
            {
                continue;
            }

            // Create working hours range for this specific day (everything in UTC)
            var workingStart = new DateTime(date.Year, date.Month, date.Day,
                dayWorkingHours.StartTime.Hour, dayWorkingHours.StartTime.Minute, 0, DateTimeKind.Utc);
            var workingEnd = new DateTime(date.Year, date.Month, date.Day,
                dayWorkingHours.EndTime.Hour, dayWorkingHours.EndTime.Minute, 0, DateTimeKind.Utc);

            // Collect time blocks and bookings separately
            var timeBlockPeriods = CollectTimeBlockPeriods(timeBlocks, date);
            var bookingPeriods = CollectBookingPeriods(bookings, date);

            // Apply buffer times to bookings only
            var bookingPeriodsWithBuffers = ApplyBufferTimesToBookings(bookingPeriods, tenant.BufferBeforeMinutes, tenant.BufferAfterMinutes);

            // Combine time blocks and buffered bookings
            var busyPeriods = timeBlockPeriods.Concat(bookingPeriodsWithBuffers).ToList();

            // Clip busy periods to working hours
            var clippedBusyPeriods = new List<(DateTime Start, DateTime End)>();
            foreach (var bp in busyPeriods)
            {
                var clippedStart = bp.Start > workingStart ? bp.Start : workingStart;
                var clippedEnd = bp.End < workingEnd ? bp.End : workingEnd;
                if (clippedStart < clippedEnd)
                {
                    clippedBusyPeriods.Add((clippedStart, clippedEnd));
                }
            }

            // Subtract busy periods from working hours
            var availableRanges = SubtractBusyPeriods(workingStart, workingEnd, clippedBusyPeriods);

            // Merge adjacent ranges
            var mergedRanges = MergeRanges(availableRanges);

            // Add to response
            foreach (var range in mergedRanges)
            {
                response.AvailableRanges.Add(new AvailableTimeRange
                {
                    Start = range.Start,
                    End = range.End
                });
            }
        }

        // Sort all ranges by start time
        response.AvailableRanges = response.AvailableRanges.OrderBy(r => r.Start).ToList();

        return response;
    }

    private static bool IsDayFree(WorkingHours workingHours)
    {
        // A day is considered free if start time equals end time
        return workingHours.StartTime == workingHours.EndTime;
    }

    private static List<(DateTime Start, DateTime End)> CollectTimeBlockPeriods(IEnumerable<TimeBlock> timeBlocks, DateTime date)
    {
        var timeBlockPeriods = new List<(DateTime, DateTime)>();

        foreach (var timeBlock in timeBlocks)
        {
            // Only include time blocks that fall on this date (all in UTC)
            if (timeBlock.StartDateTime.Date == date.Date)
            {
                timeBlockPeriods.Add((timeBlock.StartDateTime, timeBlock.EndDateTime));
            }
        }

        return timeBlockPeriods.OrderBy(bp => bp.Item1).ToList();
    }

    private static List<(DateTime Start, DateTime End)> CollectBookingPeriods(IEnumerable<Booking> bookings, DateTime date)
    {
        var bookingPeriods = new List<(DateTime, DateTime)>();

        foreach (var booking in bookings)
        {
            // Only include bookings that are Confirmed or Pending
            if (booking.BookingStatus == BookingStatus.Confirmed || booking.BookingStatus == BookingStatus.Pending)
            {
                // Check if the booking falls on this date (all in UTC)
                if (booking.StartDateTime.Date == date.Date)
                {
                    bookingPeriods.Add((booking.StartDateTime, booking.EndDateTime));
                }
            }
        }

        return bookingPeriods.OrderBy(bp => bp.Item1).ToList();
    }

    private static List<(DateTime Start, DateTime End)> ApplyBufferTimesToBookings(List<(DateTime Start, DateTime End)> busyPeriods, int bufferBeforeMinutes, int bufferAfterMinutes)
    {
        var result = new List<(DateTime, DateTime)>();

        foreach (var busyPeriod in busyPeriods)
        {
            // Expand the busy period with buffer times
            var expandedPeriod = (
                busyPeriod.Start.AddMinutes(-bufferBeforeMinutes),
                busyPeriod.End.AddMinutes(bufferAfterMinutes)
            );
            result.Add(expandedPeriod);
        }

        return result;
    }

    private static List<(DateTime Start, DateTime End)> SubtractBusyPeriods(DateTime workingStart, DateTime workingEnd, List<(DateTime Start, DateTime End)> busyPeriods)
    {
        var availableRanges = new List<(DateTime Start, DateTime End)> { (workingStart, workingEnd) };

        foreach (var busyPeriod in busyPeriods)
        {
            var newAvailableRanges = new List<(DateTime Start, DateTime End)>();

            foreach (var availableRange in availableRanges)
            {
                // Check if they overlap
                if (!(availableRange.Start < busyPeriod.End && availableRange.End > busyPeriod.Start))
                {
                    // No overlap, keep the range as is
                    newAvailableRanges.Add(availableRange);
                    continue;
                }

                // Add the part before the overlapping section, if any
                if (availableRange.Start < busyPeriod.Start)
                {
                    var beforeEnd = busyPeriod.Start;
                    if (availableRange.Start < beforeEnd)
                    {
                        newAvailableRanges.Add((availableRange.Start, beforeEnd));
                    }
                }

                // Add the part after the overlapping section, if any
                if (availableRange.End > busyPeriod.End)
                {
                    var afterStart = busyPeriod.End;
                    if (afterStart < availableRange.End)
                    {
                        newAvailableRanges.Add((afterStart, availableRange.End));
                    }
                }
            }

            availableRanges = newAvailableRanges;
        }

        return availableRanges;
    }

    private static List<(DateTime Start, DateTime End)> MergeRanges(List<(DateTime Start, DateTime End)> ranges)
    {
        if (ranges == null || ranges.Count == 0)
            return new List<(DateTime Start, DateTime End)>();

        // Sort ranges by start time
        var sortedRanges = ranges.OrderBy(r => r.Start).ToList();
        var merged = new List<(DateTime Start, DateTime End)>();
        var current = sortedRanges[0];

        for (int i = 1; i < sortedRanges.Count; i++)
        {
            var next = sortedRanges[i];

            // If ranges overlap or are adjacent, merge them
            if ((current.Start < next.End && current.End > next.Start) || current.End == next.Start)
            {
                current = (
                    current.Start < next.Start ? current.Start : next.Start,
                    current.End > next.End ? current.End : next.End
                );
            }
            else
            {
                merged.Add(current);
                current = next;
            }
        }

        merged.Add(current);
        return merged;
    }
}