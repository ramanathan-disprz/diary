using AutoMapper;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Requests;
using backend.Security.Extensions;
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
    private readonly IEventService _service;

    public EventController(IMapper mapper, ILogger<EventController> log, IEventService service)
    {
        _mapper = mapper;
        _log = log;
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<EventDto>> GetAllEvents(
        [FromQuery] DateOnly? date, 
        [FromQuery] DateOnly? start, 
        [FromQuery] DateOnly? end)
    {
        var userId = GetAuthenticatedUserId();
        IEnumerable<Event> events;

        if (date.HasValue)
        {
            _log.LogInformation("GET {Endpoint}?date={date}", URLConstants.Events, date);
            events = _service.FindAllByUserIdAndDate(userId, date.Value);
        }
        else if (start.HasValue && end.HasValue)
        {
            _log.LogInformation("GET {Endpoint}?start={start}&end={end}", URLConstants.Events, start, end);
            events = _service.FindAllByUserIdAndRange(userId, start, end);
        }
        else
        {
            throw new BadRequestException("Insufficient parameters : date or start and end date must be provided.");
        }
        return Ok(_mapper.Map<IEnumerable<EventDto>>(events));
    }

    [HttpGet("{id}", Name = "GetEvent")]
    public ActionResult<EventDto> Fetch(long id)
    {
        _log.LogInformation("GET {Endpoint}/{id}", URLConstants.Events, id);
        var userId = GetAuthenticatedUserId();
        var eventItem = _service.Fetch(userId, id);
        return Ok(_mapper.Map<EventDto>(eventItem));
    }

    [HttpPost(Name = "CreateEvent")]
    public ActionResult<EventDto> Create([FromBody] EventRequest request)
    {
        _log.LogInformation("POST {Endpoint}", URLConstants.Events);
        request.UserId = GetAuthenticatedUserId();
        var eventItem = _service.Create(request);
        return Created(URLConstants.Events, _mapper.Map<EventDto>(eventItem));
    }

    [HttpPut("{id}", Name = "UpdateEvent")]
    public ActionResult<EventDto> Update(long id, [FromBody] EventRequest request)
    {
        _log.LogInformation("PUT {Endpoint}/{id}", URLConstants.Events, id);
        request.UserId = GetAuthenticatedUserId();
        var eventItem = _service.Update(id, request);
        return Ok(_mapper.Map<EventDto>(eventItem));
    }

    [HttpDelete("{id}", Name = "DeleteEvent")]
    public IActionResult Delete(long id)
    {
        _log.LogInformation("DELETE {Endpoint}/{id} ", URLConstants.Events, id);
        var userId = GetAuthenticatedUserId();
        _service.Delete(userId, id);
        return NoContent();
    }

    private long GetAuthenticatedUserId()
    {
        var userId = HttpContext.GetUserId();
        if (userId == null)
            throw new InvalidCredentialsException(
                "Authentication failed: missing or invalid user ID in token");

        return userId.Value;
    }
}