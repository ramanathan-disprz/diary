using AutoMapper;
using backend.Dtos;
using backend.Models;
using backend.Requests;
using backend.Service;
using backend.Utils.Constants;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route(URLConstants.Events)]
public class EventController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<EventController> _log;
    private readonly EventService _service;

    public EventController(IMapper mapper, ILogger<EventController> log, EventService service)
    {
        _mapper = mapper;
        _log = log;
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<EventDto>> GetAllEvents(
        [FromQuery] long userId,
        [FromQuery] DateOnly? date)
    {
        _log.LogInformation("GET {Endpoint}?userId={userId}&date={date}", URLConstants.Events, userId, date);
        IEnumerable<Event> events;
        if (date == null)
        {
            events = _service.FindAllByUserId(userId);
        }
        else
        {
            events = _service.FindAllByUserIdAndDate(userId, date.Value);
        }

        return Ok(_mapper.Map<IEnumerable<EventDto>>(events));
    }

    [HttpGet("{id}", Name = "GetEvent")]
    public ActionResult<EventDto> Fetch(long id, [FromQuery] long userId)
    {
        _log.LogInformation("GET {Endpoint}/{id}?userId={userId}", URLConstants.Events, id, userId);
        var eventItem = _service.Fetch(userId, id);
        return Ok(_mapper.Map<EventDto>(eventItem));
    }

    [HttpPost(Name = "CreateEvent")]
    public ActionResult<EventDto> Create([FromBody] EventRequest request)
    {
        _log.LogInformation("POST {Endpoint}", URLConstants.Events);
        var eventItem = _service.Create(request);
        return Created(URLConstants.Events, _mapper.Map<EventDto>(eventItem));
    }

    [HttpPut("{id}", Name = "UpdateEvent")]
    public ActionResult<EventDto> Update(long id, [FromBody] EventRequest request)
    {
        _log.LogInformation("PUT {Endpoint}/{id}", URLConstants.Events, id);
        var eventItem = _service.Update(id, request);
        return Ok(_mapper.Map<EventDto>(eventItem));
    }

    [HttpDelete("{id}", Name = "DeleteEvent")]
    public IActionResult Delete(long id, [FromQuery] long userId)
    {
        _log.LogInformation("DELETE {Endpoint}/{id}?userId={userId} ", URLConstants.Events, id, userId);
        _service.Delete(userId, id);
        return NoContent();
    }
}