using Application.Common;
using Application.Features.Enrollments.Commands.CreateBulkEnrollment;
using Application.Features.Enrollments.Commands.DeleteEnrollment;
using Application.Features.Enrollments.Commands.UpdateEnrollment;
using Application.Features.Enrollments.DTOs;
using Application.Features.Enrollments.Queries.GetCourseEnrollments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Exceptions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnrollmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Result<BulkEnrollmentDto>>> AddEnrollments(
        [FromBody] CreateEnrollmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<Result<IReadOnlyList<EnrollmentDto>>>> GetCourseEnrollments(
        int courseId,
        CancellationToken cancellationToken)
    {
        var query =
            new GetEnrollmentsQuery(courseId);

        var result = await _mediator.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    //[HttpPut]
    //public async Task<ActionResult<Result<EnrollmentDto>>> UpdateEnrollment(
    //    [FromBody] UpdateEnrollmentCommand command,
    //    CancellationToken cancellationToken)
    //{
    //    var result = await _mediator.Send(
    //        command,
    //        cancellationToken);

    //    return Ok(result);
    //}

    [HttpDelete]
    public async Task<ActionResult<Result<bool>>> DeleteEnrollment(
        [FromBody] DeleteEnrollmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);

        }
        return Ok(result);
    }
}