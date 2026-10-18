using Application.Common;
using Application.Features.Identity.Commands.AssignRole;
using Application.Features.Identity.Commands.CreateUser;
using Application.Features.Identity.Commands.DeleteUser;
using Application.Features.Identity.Commands.RemoveRole;
using Application.Features.Identity.Commands.ToggleUserStatus;
using Application.Features.Identity.Commands.UpdateUser;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Queries.GetCurrentUser;
using Application.Features.Identity.Queries.GetEmails;
using Application.Features.Identity.Queries.GetUserById;
using Application.Features.Identity.Queries.GetUsers;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Exceptions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = AppRoles.Admin)]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<UserListItemDto>>>> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }
    [HttpGet("emails")]
    public async Task<ActionResult<Result<IReadOnlyList<EnrollmentEmailsDto>>>> GetUsersEmail(
    [FromQuery] int courseId,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEnrollmentEmailsQuery(courseId), cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<UserDetailDto>>> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserByIdQuery { Id = id },
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<Result<UserDetailDto>>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCurrentUserQuery(),
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<UserDetailDto>>> CreateUser(
        [FromBody] CreateUserCommand command,
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<UserDetailDto>>> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> DeleteUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteUserCommand { Id = id },
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<Result<UserDetailDto>>> ToggleUserStatus(
        [FromRoute] Guid id,
        [FromBody] ToggleUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<Result>> AssignRole(
        [FromRoute] Guid id,
        [FromBody] AssignRoleCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = id;
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}/roles/{roleName}")]
    public async Task<ActionResult<Result>> RemoveRole(
        [FromRoute] Guid id,
        [FromRoute] string roleName,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveRoleCommand { UserId = id, RoleName = roleName },
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }
}
