using Application.Common;
using Application.Features.Lessons.DTOs;
using MediatR;

namespace Application.Features.Lessons.Queries.GetLessonAccess;

public class GetLessonAccessQuery : IRequest<Result<LessonAccessDto>>
{
    public int LessonId { get; set; }

    public GetLessonAccessQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
