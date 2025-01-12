﻿using MediatR;
using Shoping.Application.Common.Interfaces;
using Shoping.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shoping.WebApi.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPost]
    public Task<TokenCommandResponse> Token([FromBody] TokenCommand command) =>
        _mediator.Send(command);

    [HttpPost("refresh")]
    public Task<RefreshTokenCommandResponse> RefreshToken([FromBody] RefreshTokenCommand command) =>
        _mediator.Send(command);


    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUserService currentUser)
    {
        return Ok(new
        {
            currentUser.User,
            IsAdmin = currentUser.IsInRole("Admin")
        });
    }
}
