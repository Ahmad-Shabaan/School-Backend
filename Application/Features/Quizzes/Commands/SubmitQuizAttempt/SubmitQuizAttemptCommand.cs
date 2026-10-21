using Application.Common;
using Application.Features.Quizzes.DTOs;
using MediatR;

namespace Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public class SubmitQuizAttemptCommand : IRequest<Result<QuizAttemptDto>>
{
    public int LessonId { get; set; }

    public List<QuizAnswerInput> Answers { get; set; } = new();
}
