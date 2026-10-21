using Application.Common;
using Application.Features.Watermark.DTOs;
using MediatR;

namespace Application.Features.Watermark.Queries.GetLessonWatermark;

public class GetLessonWatermarkQuery : IRequest<Result<WatermarkDto>>
{
    public int LessonId { get; set; }

    public GetLessonWatermarkQuery(int lessonId)
    {
        LessonId = lessonId;
    }
}
