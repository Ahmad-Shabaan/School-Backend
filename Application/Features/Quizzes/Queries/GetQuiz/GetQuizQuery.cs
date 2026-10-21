using Application.Common;
using Application.Features.Quizzes.DTOs;
using MediatR;

namespace Application.Features.Quizzes.Queries.GetQuiz;

public class GetQuizQuery : IRequest<Result<QuizDto>>
{
    public int LessonId { get; set; }

    public GetQuizQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
