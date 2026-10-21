using Application.Common;
using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Exceptions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Result<CommentDto>>> CreateComment(
        [FromBody] CreateCommentCommand command,
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<CommentDto>>> UpdateComment(
        int id,
        [FromBody] UpdateCommentCommand command,
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

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> DeleteComment(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteCommentCommand(id),
            cancellationToken);

        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }

        return Ok(result);
    }
}
