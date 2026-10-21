using Application.Common;
using MediatR;

namespace Application.Features.Quizzes.Commands.DeleteQuiz;

public class DeleteQuizCommand : IRequest<Result>
{
    public int LessonId { get; set; }

    public DeleteQuizCommand(int lessonId)
    {
        LessonId = lessonId;
    }
}
