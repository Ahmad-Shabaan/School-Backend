using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Queries.GetQuiz;

public class GetQuizQueryHandler
    : IRequestHandler<GetQuizQuery, Result<QuizDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetQuizQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<QuizDto>> Handle(
        GetQuizQuery request,
        CancellationToken cancellationToken)
    {
        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonWithQuestionsSpecification(
                    request.LessonId),
                cancellationToken);

        if (quiz is null)
            return Result<QuizDto>.Failed(
                "Quiz was not found.",
                QuizErrors.QuizNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            _currentUserService.UserId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<QuizDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var response = new QuizDto
        {
            Id = quiz.Id,
            LessonId = quiz.LessonId,
            Title = quiz.Title,
            PassingScorePercent = quiz.PassingScorePercent,
            Questions = quiz.Questions
                .OrderBy(q => q.Id)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options
                        .OrderBy(o => o.Id)
                        .Select(o => new AnswerOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text
                        })
                        .ToList()
                })
                .ToList()
        };

        return Result<QuizDto>.Succeeded(response);
    }
}
