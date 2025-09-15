using AutoMapper;
using backend.Dtos;
using backend.Models;
using backend.Requests;
using backend.Service;
using backend.Utils.Constants;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route(URLConstants.Users)]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly UserService _service;
    private readonly ILogger<UserController> _log;

    public UserController(IMapper mapper, UserService service, ILogger<UserController> log)
    {
        _log = log;
        _mapper = mapper;
        _service = service;
    }

    [HttpGet(Name = "GetAllUsers")]
    public ActionResult<IEnumerable<UserDto>> Index()
    {
        _log.LogInformation("GET {Endpoint}", URLConstants.Users);
        IEnumerable<User> users = _service.Index();
        return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }

    [HttpGet("{id}", Name = "GetUser")]
    public ActionResult<UserDto> Fetch(long id)
    {
        _log.LogInformation("GET {Endpoint}/{id}", URLConstants.Users, id);
        var user = _service.Fetch(id);
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut("{id}", Name = "UpdateUser")]
    public ActionResult<UserDto> Update(long id, [FromBody] UserRequest request)
    {
        _log.LogInformation("PUT {Endpoint}/{id}", URLConstants.Users, id);
        var user = _service.Update(id, request);
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpDelete("{id}", Name = "DeleteUser")]
    public IActionResult Delete(long id)
    {
        _log.LogInformation("DELETE {Endpoint}/{id}", URLConstants.Users, id);
        _service.Delete(id);
        return NoContent();
    }
}