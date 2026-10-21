using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Quizzes.Commands.CreateQuiz;
using Application.Features.Quizzes.DTOs;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Commands.UpdateQuiz;

public class UpdateQuizCommandHandler
    : IRequestHandler<UpdateQuizCommand, Result<QuizManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateQuizCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<QuizManagementDto>> Handle(
        UpdateQuizCommand request,
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

        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonWithQuestionsSpecification(
                    request.LessonId),
                cancellationToken);

        if (quiz is null)
            return Result<QuizManagementDto>.Failed(
                "Quiz was not found.",
                QuizErrors.QuizNotFound);

        var attemptCount = await _unitOfWork
            .Repository<QuizAttempt>()
            .CountAsync(
                new QuizAttemptsByQuizSpecification(quiz.Id),
                cancellationToken);

        if (attemptCount > 0 &&
            !QuestionsEquivalent(quiz, request))
        {
            return Result<QuizManagementDto>.Failed(
                "The quiz questions cannot be changed after students have submitted attempts. Only the title and passing score may be updated.",
                QuizErrors.HasAttempts);
        }

        quiz.Title = request.Title.Trim();
        quiz.PassingScorePercent = request.PassingScorePercent;

        if (attemptCount == 0)
        {
            var previousQuestions = quiz.Questions.ToList();

            foreach (var previous in previousQuestions)
            {
                foreach (var option in previous.Options.ToList())
                {
                    _unitOfWork
                        .Repository<AnswerOption>()
                        .Delete(option);
                }

                _unitOfWork
                    .Repository<Question>()
                    .Delete(previous);
            }

            var replacement = CreateQuizCommandHandler.BuildQuizGraph(
                request.LessonId,
                request);

            foreach (var question in replacement.Questions)
            {
                quiz.Questions.Add(question);
            }
        }

        _unitOfWork
            .Repository<Quiz>()
            .Update(quiz);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<QuizManagementDto>.Succeeded(
            CreateQuizCommandHandler.MapManagement(quiz, attemptCount),
            "Quiz updated successfully.");
    }

    internal static bool QuestionsEquivalent(
        Quiz quiz,
        IQuizPayload payload)
    {
        var storedQuestions = quiz.Questions
            .OrderBy(q => q.Id)
            .ToList();

        if (storedQuestions.Count != payload.Questions.Count)
            return false;

        for (var i = 0; i < storedQuestions.Count; i++)
        {
            var stored = storedQuestions[i];
            var requested = payload.Questions[i];

            if (!string.Equals(
                    stored.Text.Trim(),
                    requested.Text.Trim(),
                    StringComparison.Ordinal))
                return false;

            var storedOptions = stored.Options
                .OrderBy(o => o.Id)
                .ToList();

            if (storedOptions.Count != requested.Options.Count)
                return false;

            for (var j = 0; j < storedOptions.Count; j++)
            {
                if (!string.Equals(
                        storedOptions[j].Text.Trim(),
                        requested.Options[j].Text.Trim(),
                        StringComparison.Ordinal) ||
                    storedOptions[j].IsCorrect !=
                        requested.Options[j].IsCorrect)
                    return false;
            }
        }

        return true;
    }
}
