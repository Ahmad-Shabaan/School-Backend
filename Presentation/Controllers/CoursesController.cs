using Application.Common;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Commands.DeleteCourse;
using Application.Features.Courses.Commands.UpdateCourse;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Queries.GetCourse;
using Application.Features.Courses.Queries.GetCourses;
using Application.Features.Courses.Queries.GetUserCourses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Exceptions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<CourseDto>>> GetCourses(
    CancellationToken cancellationToken)
    {

        var result = await _mediator.Send(new GetCoursesQuery(), cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }

    [HttpPost("create-course")]
    public async Task<ActionResult<Result<CourseDto>>> CreateCourse(
        [FromForm] CreateCourseCommand command,
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

    [HttpGet("user-courses")]
    public async Task<ActionResult<Result<List<CourseDto>>>> GetUserCourses(
        [FromHeader] string userEmail,
        CancellationToken cancellationToken)
    {
        var query = new GetUserCoursesQuery(userEmail);

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<CourseDto>>> GetCourse(
        int id,
        CancellationToken cancellationToken)
    {
        var query = new GetCourseQuery(id);

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<CourseDto>>> UpdateCourse(
        int id,
        [FromForm] UpdateCourseCommand command,
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
    public async Task<ActionResult<Result<bool>>> DeleteCourse(
        int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand(id);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }
        return Ok(result);
    }
}