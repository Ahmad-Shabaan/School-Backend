using Application.Common;
using MediatR;

namespace Application.Features.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommand : IRequest<Result>
{
    public int Id { get; set; }

    public DeleteLessonCommand(int id)
    {
        Id = id;
    }
}
