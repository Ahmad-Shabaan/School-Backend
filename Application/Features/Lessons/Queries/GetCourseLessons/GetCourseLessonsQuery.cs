using Application.Common;
using Application.Features.Lessons.DTOs;
using MediatR;

namespace Application.Features.Lessons.Queries.GetCourseLessons;

public class GetCourseLessonsQuery : IRequest<Result<List<LessonStatusDto>>>
{
    public int CourseId { get; set; }

    public GetCourseLessonsQuery(int courseId)
    {
        CourseId = courseId;
    }
}
