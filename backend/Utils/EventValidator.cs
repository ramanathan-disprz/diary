using backend.Exceptions;
using backend.Models;
using backend.Requests;

namespace backend.Utils;

public static class EventValidator
{
    public static void ValidateRequest(EventRequest request)
    {
        if (request.Date == null)
            throw new BadRequestException("Event Date cannot be null.");

        if (request.StartTime == null)
            throw new BadRequestException("Start Time cannot be null.");

        if (request.EndTime == null)
            throw new BadRequestException("End Time cannot be null.");

        if (request.EndTime <= request.StartTime)
            throw new BadRequestException("End Time must be greater than Start Time.");

        if (request.Date < new DateOnly(1900, 1, 1))
            throw new BadRequestException("Event Date cannot be earlier than year 1900.");
    }

    public static void EnsureNoConflict(EventRequest request, IEnumerable<Event> eventsOnSameDay)
    {
        var startTime = request.StartTime;
        var endTime = request.EndTime;

        var conflictExists = eventsOnSameDay.Any(existingEvent =>
            (startTime < existingEvent.EndTime && endTime > existingEvent.StartTime));

        if (conflictExists)
            throw new ConflictException("Cannot create Event due to a scheduling conflict");
    }
}