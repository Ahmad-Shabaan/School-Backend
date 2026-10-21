using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Commands.CreateQuiz;

public class CreateQuizCommandHandler
    : IRequestHandler<CreateQuizCommand, Result<QuizManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuizCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<QuizManagementDto>> Handle(
        CreateQuizCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result<QuizManagementDto>.Failed(
                "You are not authorized to manage quizzes.",
                LessonErrors.NotAuthorized);

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<QuizManagementDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var existing = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonSpecification(request.LessonId),
                cancellationToken);

        if (existing is not null)
            return Result<QuizManagementDto>.Failed(
                "A quiz already exists for this lesson.",
                QuizErrors.QuizAlreadyExists);

        var quiz = BuildQuizGraph(request.LessonId, request);

        await _unitOfWork
            .Repository<Quiz>()
            .AddAsync(quiz, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<QuizManagementDto>.Succeeded(
            MapManagement(quiz, attemptCount: 0),
            "Quiz created successfully.");
    }

    internal static Quiz BuildQuizGraph(int lessonId, IQuizPayload payload)
    {
        var quiz = new Quiz
        {
            LessonId = lessonId,
            Title = payload.Title.Trim(),
            PassingScorePercent = payload.PassingScorePercent
        };

        foreach (var questionInput in payload.Questions)
        {
            var question = new Question
            {
                Text = questionInput.Text.Trim()
            };

            foreach (var optionInput in questionInput.Options)
            {
                question.Options.Add(new AnswerOption
                {
                    Text = optionInput.Text.Trim(),
                    IsCorrect = optionInput.IsCorrect
                });
            }

            quiz.Questions.Add(question);
        }

        return quiz;
    }

    internal static QuizManagementDto MapManagement(
        Quiz quiz,
        int attemptCount)
    {
        return new QuizManagementDto
        {
            Id = quiz.Id,
            LessonId = quiz.LessonId,
            Title = quiz.Title,
            PassingScorePercent = quiz.PassingScorePercent,
            AttemptCount = attemptCount,
            Questions = quiz.Questions
                .OrderBy(q => q.Id)
                .Select(q => new QuizManagementQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Options
                        .OrderBy(o => o.Id)
                        .Select(o => new QuizManagementOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
