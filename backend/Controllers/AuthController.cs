using AutoMapper;
using backend.Dtos;
using backend.Requests;
using backend.Service;
using backend.Utils.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route(URLConstants.Auth)]
public class AuthController : ControllerBase
{
    private IMapper _mapper;
    private IAuthService _service;
    private ILogger<AuthController> _log;

    public AuthController(IMapper mapper, IAuthService service, ILogger<AuthController> log)
    {
        _log = log;
        _mapper = mapper;
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost("register", Name = "RegisterUser")]
    public ActionResult<UserDto> Create([FromBody] UserRequest request)
    {
        _log.LogInformation("POST {Endpoint}", URLConstants.Auth + "/register");
        var user = _service.Register(request);
        return Created(URLConstants.Auth + "/register", _mapper.Map<UserDto>(user));
    }

    [AllowAnonymous]
    [HttpPost("login", Name = "LoginUser")]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginRequest request)
    {
        _log.LogInformation("POST {Endpoint}", URLConstants.Auth + "/login");
        return _service.Login(request);
    }
}