using Application.Common;
using Application.Features.Quizzes.DTOs;
using MediatR;

namespace Application.Features.Quizzes.Queries.GetQuizAttempts;

public class GetQuizAttemptsQuery : IRequest<Result<List<QuizAttemptDto>>>
{
    public int LessonId { get; set; }

    public GetQuizAttemptsQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
