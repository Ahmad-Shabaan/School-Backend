using Application.Common;
using Application.Features.Progress.DTOs;
using MediatR;

namespace Application.Features.Progress.Queries.GetLessonProgress;

public class GetLessonProgressQuery : IRequest<Result<LessonProgressDto>>
{
    public int LessonId { get; set; }

    public GetLessonProgressQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
