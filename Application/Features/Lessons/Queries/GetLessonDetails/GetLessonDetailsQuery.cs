using Application.Common;
using Application.Features.Lessons.DTOs;
using MediatR;

namespace Application.Features.Lessons.Queries.GetLessonDetails;

public class GetLessonDetailsQuery : IRequest<Result<LessonDetailsDto>>
{
    public int LessonId { get; set; }

    public GetLessonDetailsQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
