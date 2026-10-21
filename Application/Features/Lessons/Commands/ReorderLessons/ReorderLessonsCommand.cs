using Application.Common;
using MediatR;

namespace Application.Features.Lessons.Commands.ReorderLessons;

public class ReorderLessonsCommand : IRequest<Result>
{
    public int CourseId { get; set; }

    public List<int> OrderedLessonIds { get; set; } = new();
}
