using System.Text.Json;
using AutoMapper;
using backend.Exceptions;
using backend.Models;
using backend.Repository;
using backend.Requests;
using backend.Utils;

namespace backend.Service;

public class EventService
{
    private readonly IMapper _mapper;
    private readonly ILogger<EventService> _log;
    private readonly IEventRepository _repository;

    public EventService(IMapper mapper, ILogger<EventService> log, IEventRepository repository)
    {
        _mapper = mapper;
        _log = log;
        _repository = repository;
    }

    public IEnumerable<Event> FindAllByUserIdAndDate(long? userId, DateOnly? date)
    {
        if (!date.HasValue)
        {
            throw new BadRequestException("Insufficient parameters : date must be provided.");
        }

        _log.LogInformation("Find events on date : {date} for the user : {userId}", date, userId);
        return _repository.FindAllByUserIdAndDate(userId, date);
    }

    public IEnumerable<Event> FindAllByUserIdAndRange(long? userId, DateOnly? start, DateOnly? end)
    {
        if (!start.HasValue || !end.HasValue)
        {
            throw new BadRequestException("Insufficient parameters : start date and end date must be provided.");
        }

        _log.LogInformation("Find events on range : {start} - {end} for the user : {userId}", start, end, userId);
        return _repository.FindAllByUserIdAndRange(userId, start, end);
    }

    public Event Fetch(long? userId, long id)
    {
        _log.LogInformation("Find event with id : {eventId} and user with id: {userId}", id, userId);
        return _repository.FindByUserIdAndIdOrThrow(userId, id);
    }

    public Event Create(EventRequest request)
    {
        _log.LogInformation("Create new event : {eventRequest}", JsonSerializer.Serialize(request));
        var eventItem = _mapper.Map<Event>(request);
        ValidateEvent(eventItem);
        EnsureNoConflict(eventItem);
        eventItem.GenerateId();
        return _repository.Create(eventItem);
    }

    public Event Update(long id, EventRequest request)
    {
        _log.LogInformation("Updating event with id: {EventId} " +
                            "and request: {eventRequest}", id, JsonSerializer.Serialize(request));
        var eventItem = Fetch(request.UserId, id);
        eventItem = _mapper.Map(request, eventItem);
        ValidateEvent(eventItem);
        EnsureNoConflict(eventItem);
        return _repository.Update(eventItem);
    }

    public void Delete(long userId, long id)
    {
        _log.LogInformation("Delete event with id : {eventId}", id);
        var eventItem = Fetch(userId, id);
        _repository.Delete(eventItem);
    }

    private void ValidateEvent(Event eventItem)
    {
        EventValidator.ValidateEvent(eventItem);
    }

    private void EnsureNoConflict(Event eventItem)
    {
        var eventsOnSameDay = FindAllByUserIdAndDate(eventItem.UserId, eventItem.StartDate);
        EventValidator.EnsureNoConflict(eventItem, eventsOnSameDay);
    }
}