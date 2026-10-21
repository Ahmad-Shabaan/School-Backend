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

namespace Application.Features.Quizzes.Queries.GetQuizManagement;

public class GetQuizManagementQuery
    : IRequest<Result<QuizManagementDto>>
{
    public int LessonId { get; set; }

    public GetQuizManagementQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}

public class GetQuizManagementQueryHandler
    : IRequestHandler<GetQuizManagementQuery, Result<QuizManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetQuizManagementQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<QuizManagementDto>> Handle(
        GetQuizManagementQuery request,
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

        return Result<QuizManagementDto>.Succeeded(
            CreateQuizCommandHandler.MapManagement(quiz, attemptCount));
    }
}
