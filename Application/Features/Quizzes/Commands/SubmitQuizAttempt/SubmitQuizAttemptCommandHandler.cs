using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public class SubmitQuizAttemptCommandHandler
    : IRequestHandler<SubmitQuizAttemptCommand, Result<QuizAttemptDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public SubmitQuizAttemptCommandHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<QuizAttemptDto>> Handle(
        SubmitQuizAttemptCommand request,
        CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;

        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonWithQuestionsSpecification(
                    request.LessonId),
                cancellationToken);

        if (quiz is null)
            return Result<QuizAttemptDto>.Failed(
                "Quiz was not found.",
                QuizErrors.QuizNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            studentId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<QuizAttemptDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var questionIds = quiz.Questions.Select(q => q.Id).ToHashSet();

        var submittedQuestionIds = request.Answers
            .Select(a => a.QuestionId)
            .ToList();

        if (submittedQuestionIds.Count != submittedQuestionIds.Distinct().Count())
            return Result<QuizAttemptDto>.Failed(
                "A question cannot be answered more than once.",
                QuizErrors.DuplicateAnswers);

        if (submittedQuestionIds.Count != questionIds.Count ||
            !questionIds.SetEquals(submittedQuestionIds))
            return Result<QuizAttemptDto>.Failed(
                "All quiz questions must be answered exactly once.",
                QuizErrors.IncompleteAnswers);

        var optionsByQuestion = quiz.Questions
            .ToDictionary(
                q => q.Id,
                q => q.Options.ToDictionary(o => o.Id, o => o.IsCorrect));

        foreach (var answer in request.Answers)
        {
            if (!optionsByQuestion.TryGetValue(answer.QuestionId,out var optionMap) ||
                !optionMap.ContainsKey(answer.OptionId))
            {
                return Result<QuizAttemptDto>.Failed(
                    "One or more submitted answers are invalid for this quiz.",
                    QuizErrors.InvalidAnswers);
            }
        }

        var correctCount = request.Answers.Count(a =>
            optionsByQuestion[a.QuestionId][a.OptionId]);

        var score = Math.Round(
            (decimal)correctCount / questionIds.Count * 100m,
            2);

        var isPassed = score >= quiz.PassingScorePercent;

        var previousAttempts = await _unitOfWork
            .Repository<QuizAttempt>()
            .ListAsync(
                new QuizAttemptsByStudentAndQuizSpecification(
                    studentId,
                    quiz.Id),
                cancellationToken);

        var attemptNumber = previousAttempts.Count == 0
            ? 1
            : previousAttempts.Max(a => a.AttemptNumber) + 1;

        var attempt = new QuizAttempt
        {
            ApplicationUserId = studentId,
            QuizId = quiz.Id,
            AttemptNumber = attemptNumber,
            StartedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow,
            Score = score,
            IsPassed = isPassed
        };

        foreach (var answer in request.Answers)
        {
            var isCorrect = optionsByQuestion[answer.QuestionId][answer.OptionId];

            attempt.Answers.Add(new QuizAttemptAnswer
            {
                QuestionId = answer.QuestionId,
                SelectedOptionId = answer.OptionId,
                PointsAwarded = isCorrect ? 1m : 0m
            });
        }

        await _unitOfWork
            .Repository<QuizAttempt>()
            .AddAsync(attempt, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new QuizAttemptDto
        {
            Id = attempt.Id,
            QuizId = attempt.QuizId,
            AttemptNumber = attempt.AttemptNumber,
            Score = attempt.Score,
            IsPassed = attempt.IsPassed,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Answers = attempt.Answers
                .Select(a => new QuizAttemptAnswerDto
                {
                    QuestionId = a.QuestionId,
                    SelectedOptionId = a.SelectedOptionId,
                    PointsAwarded = a.PointsAwarded
                })
                .ToList()
        };

        return Result<QuizAttemptDto>.Succeeded(
            response,
            "Quiz attempt submitted successfully.");
    }
}
