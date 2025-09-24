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
[Route(URLConstants.Users)]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _service;
    private readonly ILogger<UserController> _log;

    public UserController(IMapper mapper, IUserService service, ILogger<UserController> log)
    {
        _log = log;
        _mapper = mapper;
        _service = service;
    }
    
    // TODO :: This must not be opened
    /*
    [HttpGet(Name = "GetAllUsers")]
    public ActionResult<IEnumerable<UserDto>> Index()
    {
        _log.LogInformation("GET {Endpoint}", URLConstants.Users);
        IEnumerable<User> users = _service.Index();
        return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }
    */

    [HttpGet("me", Name = "GetUser")]
    public ActionResult<UserDto> Fetch()
    {
        _log.LogInformation("GET {Endpoint}/me", URLConstants.Users);
        var id = GetAuthenticatedUserId();
        var user = _service.Fetch(id);
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut(Name = "UpdateUser")]
    public ActionResult<UserDto> Update([FromBody] UserRequest request)
    {
        _log.LogInformation("PUT {Endpoint}", URLConstants.Users);
        var id = GetAuthenticatedUserId();
        var user = _service.Update(id, request);
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpDelete(Name = "DeleteUser")]
    public IActionResult Delete()
    {
        _log.LogInformation("DELETE {Endpoint}", URLConstants.Users);
        var id = GetAuthenticatedUserId();
        _service.Delete(id);
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