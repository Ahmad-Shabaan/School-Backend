using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Queries.GetQuizAttempts;

public class GetQuizAttemptsQueryHandler
    : IRequestHandler<GetQuizAttemptsQuery, Result<List<QuizAttemptDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetQuizAttemptsQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<QuizAttemptDto>>> Handle(
        GetQuizAttemptsQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;

        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonSpecification(request.LessonId),
                cancellationToken);

        if (quiz is null)
            return Result<List<QuizAttemptDto>>.Failed(
                "Quiz was not found.",
                QuizErrors.QuizNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            studentId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<List<QuizAttemptDto>>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var attempts = await _unitOfWork
            .Repository<QuizAttempt>()
            .ListAsync(
                new QuizAttemptsWithAnswersByStudentAndQuizSpecification(
                    studentId,
                    quiz.Id),
                cancellationToken);

        var response = attempts
            .Select(a => new QuizAttemptDto
            {
                Id = a.Id,
                QuizId = a.QuizId,
                AttemptNumber = a.AttemptNumber,
                Score = a.Score,
                IsPassed = a.IsPassed,
                StartedAt = a.StartedAt,
                SubmittedAt = a.SubmittedAt,
                Answers = a.Answers
                    .Select(ans => new QuizAttemptAnswerDto
                    {
                        QuestionId = ans.QuestionId,
                        SelectedOptionId = ans.SelectedOptionId,
                        PointsAwarded = ans.PointsAwarded
                    })
                    .ToList()
            })
            .ToList();

        return Result<List<QuizAttemptDto>>.Succeeded(response);
    }
}
