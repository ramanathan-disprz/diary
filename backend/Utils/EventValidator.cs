using backend.Exceptions;
using backend.Models;
using backend.Requests;

namespace backend.Utils;

public static class EventValidator
{
    public static void ValidateEvent(Event eventItem)
    {
        if (eventItem.EndTime <= eventItem.StartTime)
            throw new BadRequestException("End Time must be greater than Start Time.");

        if (eventItem.StartDate < new DateOnly(1900, 1, 1))
            throw new BadRequestException("Event Date cannot be earlier than year 1900.");
    }

    public static void EnsureNoConflict(Event eventItem, IEnumerable<Event> eventsOnSameDay)
    {
        var startTime = eventItem.StartTime;
        var endTime = eventItem.EndTime;

        var conflictExists = eventsOnSameDay
            .Where(existingEvent => existingEvent.Id != eventItem.Id)
            .Any(existingEvent => (startTime < existingEvent.EndTime && endTime > existingEvent.StartTime));

        if (conflictExists)
            throw new ConflictException("Cannot create Event due to a scheduling conflict");
    }
}