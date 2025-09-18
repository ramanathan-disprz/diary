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

    public IEnumerable<Event> FindAllByUserId(long userId)
    {
        _log.LogInformation("Find events for the user : {userId}", userId);
        var events =  _repository.FindAllByUserId(userId);
        return events;
    }

    public IEnumerable<Event> FindAllByUserIdAndDate(long? userId, DateOnly? date)
    {
        _log.LogInformation("Find events on date : {Date} for the user : {userId}", date, userId);
        return _repository.FindByUserIdAndDate(userId, date);
    }

    public Event Fetch(long? userId, long id)
    {
        _log.LogInformation("Find event with id : {eventId} and user with id: {userId}", id, userId);
        return _repository.FindByUserIdAndIdOrThrow(userId, id);
    }

    public Event Create(EventRequest request)
    {
        _log.LogInformation("Create new event : {eventRequest}", JsonSerializer.Serialize(request));
        ValidateRequest(request);
        EnsureNoConflict(request);
        var eventItem = _mapper.Map<Event>(request);
        eventItem.GenerateId();
        return _repository.Create(eventItem);
    }

    public Event Update(long id, EventRequest request)
    {
        _log.LogInformation("Updating event with id: {EventId} " +
                            "and request: {eventRequest}", id, JsonSerializer.Serialize(request));
        var eventItem = Fetch(request.UserId, id);
        eventItem = _mapper.Map(request, eventItem);
        return _repository.Update(eventItem);
    }

    public void Delete(long userId, long id)
    {
        _log.LogInformation("Delete event with id : {eventId}", id);
        var eventItem = Fetch(userId, id);
        _repository.Delete(eventItem);
    }

    private void ValidateRequest(EventRequest request)
    {
        EventValidator.ValidateRequest(request);
    }

    private void EnsureNoConflict(EventRequest request)
    {
        var eventsOnSameDay = FindAllByUserIdAndDate(request.UserId, request.Date);
        EventValidator.EnsureNoConflict(request, eventsOnSameDay);
    }
}