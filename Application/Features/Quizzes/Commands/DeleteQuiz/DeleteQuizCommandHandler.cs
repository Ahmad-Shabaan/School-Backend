using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Quizzes.Errors;
using Application.Features.Quizzes.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Quizzes.Commands.DeleteQuiz;

public class DeleteQuizCommandHandler
    : IRequestHandler<DeleteQuizCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteQuizCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeleteQuizCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsStaff())
            return Result.Failed(
                "You are not authorized to manage quizzes.",
                LessonErrors.NotAuthorized);

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonSpecification(request.LessonId),
                cancellationToken);

        if (quiz is null)
            return Result.Failed(
                "Quiz was not found.",
                QuizErrors.QuizNotFound);

        var attemptCount = await _unitOfWork
            .Repository<QuizAttempt>()
            .CountAsync(
                new QuizAttemptsByQuizSpecification(quiz.Id),
                cancellationToken);

        if (attemptCount > 0)
            return Result.Failed(
                "The quiz cannot be deleted after students have submitted attempts.",
                QuizErrors.HasAttempts);

        _unitOfWork
            .Repository<Quiz>()
            .Delete(quiz);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Succeeded("Quiz deleted successfully.");
    }
}
