using Application.Common;
using Application.Features.Authentication.Commands.ChangePassword;
using Application.Features.Authentication.Commands.Login;
using Application.Features.Authentication.Commands.logout;
using Application.Features.Authentication.Commands.Refresh;
using Application.Features.Authentication.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Presentation.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }


    [HttpDelete("logout")]
    public async Task<ActionResult> Logout([FromHeader]string RefreshToken, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return Unauthorized(problem);
        }
        return Ok();

    }


    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }


    [HttpPost("refresh-token")]
    public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer "))
            return Unauthorized();

        var refreshToken = authorization["Bearer ".Length..].Trim();
        var command = new RefreshTokenCommand() { RefresherToken = refreshToken };
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }
}
