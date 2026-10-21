using Application.Common;
using Application.Features.Comments.DTOs;
using Application.Features.Comments.Queries.GetLessonComments;
using Application.Features.Lessons.Commands.CreateLesson;
using Application.Features.Lessons.Commands.DeleteLesson;
using Application.Features.Lessons.Commands.ReorderLessons;
using Application.Features.Lessons.Commands.UpdateLesson;
using Application.Features.Lessons.DTOs;
using Application.Features.Lessons.Queries.GetCourseLessons;
using Application.Features.Lessons.Queries.GetLessonAccess;
using Application.Features.Lessons.Queries.GetLessonDetails;
using Application.Features.Progress.Commands.SubmitProgressReport;
using Application.Features.Progress.DTOs;
using Application.Features.Progress.Queries.GetLessonProgress;
using Application.Features.Quizzes.Commands.CreateQuiz;
using Application.Features.Quizzes.Commands.DeleteQuiz;
using Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using Application.Features.Quizzes.Commands.UpdateQuiz;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Queries.GetQuiz;
using Application.Features.Quizzes.Queries.GetQuizAttempts;
using Application.Features.Quizzes.Queries.GetQuizManagement;
using Application.Features.Watermark.DTOs;
using Application.Features.Watermark.Queries.GetLessonWatermark;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Exceptions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LessonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<Result<List<LessonStatusDto>>>> GetCourseLessons(
        int courseId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCourseLessonsQuery(courseId),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<LessonDetailsDto>>> GetLesson(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonDetailsQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/access")]
    public async Task<ActionResult<Result<LessonAccessDto>>> GetLessonAccess(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonAccessQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<LessonDetailsDto>>> CreateLesson(
        [FromBody] CreateLessonCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<LessonDetailsDto>>> UpdateLesson(
        int id,
        [FromBody] UpdateLessonCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("reorder")]
    public async Task<ActionResult<Result>> ReorderLessons(
        [FromBody] ReorderLessonsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> DeleteLesson(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteLessonCommand(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/progress")]
    public async Task<ActionResult<Result<LessonProgressDto>>> GetProgress(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonProgressQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("{id:int}/progress")]
    public async Task<ActionResult<Result<LessonProgressDto>>> SubmitProgress(
        int id,
        [FromBody] SubmitProgressReportCommand command,
        CancellationToken cancellationToken)
    {
        command.LessonId = id;

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/quiz")]
    public async Task<ActionResult<Result<QuizDto>>> GetQuiz(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetQuizQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("{id:int}/quiz/attempts")]
    public async Task<ActionResult<Result<QuizAttemptDto>>> SubmitQuizAttempt(
        int id,
        [FromBody] SubmitQuizAttemptCommand command,
        CancellationToken cancellationToken)
    {
        command.LessonId = id;

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/quiz/attempts")]
    public async Task<ActionResult<Result<List<QuizAttemptDto>>>> GetQuizAttempts(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetQuizAttemptsQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("{id:int}/quiz")]
    public async Task<ActionResult<Result<QuizManagementDto>>> CreateQuiz(
        int id,
        [FromBody] CreateQuizCommand command,
        CancellationToken cancellationToken)
    {
        command.LessonId = id;

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{id:int}/quiz")]
    public async Task<ActionResult<Result<QuizManagementDto>>> UpdateQuiz(
        int id,
        [FromBody] UpdateQuizCommand command,
        CancellationToken cancellationToken)
    {
        command.LessonId = id;

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpDelete("{id:int}/quiz")]
    public async Task<ActionResult<Result>> DeleteQuiz(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteQuizCommand(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/quiz/manage")]
    public async Task<ActionResult<Result<QuizManagementDto>>> GetQuizForManagement(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetQuizManagementQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<Result<List<CommentDto>>>> GetComments(
        int id,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetLessonCommentsQuery(id, pageIndex, pageSize),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{id:int}/watermark")]
    public async Task<ActionResult<Result<WatermarkDto>>> GetWatermark(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLessonWatermarkQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    private ObjectResult HandleResult<T>(Result<T> result)
    {
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }

        return Ok(result);
    }

    private ObjectResult HandleResult(Result result)
    {
        if (!result.Success)
        {
            var problem = GlobalErrorsHandler.Handle(result);
            return StatusCode(problem.Status!.Value, problem);
        }

        return Ok(result);
    }
}
