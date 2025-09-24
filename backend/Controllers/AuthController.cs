using AutoMapper;
using backend.Dtos;
using backend.Exceptions;
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
        handleNullRequests(request);
        checkModelState();
        var user = _service.Register(request);
        return Created(URLConstants.Auth + "/register", _mapper.Map<UserDto>(user));
    }

    [AllowAnonymous]
    [HttpPost("login", Name = "LoginUser")]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginRequest request)
    {
        _log.LogInformation("POST {Endpoint}", URLConstants.Auth + "/login");
        handleNullRequests(request);
        checkModelState();
        return _service.Login(request);
    }


    private void handleNullRequests<T>(T request)
    {
        if (request == null)
        {
            throw new BadRequestException("Request cannot be null");
        }
    }

    private void checkModelState()
    {
        if (!ModelState.IsValid)
        {
            throw new BadRequestException("ModelState not valid");
        }
    }
}