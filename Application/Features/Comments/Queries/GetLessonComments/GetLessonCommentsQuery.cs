using Application.Common;
using Application.Features.Comments.DTOs;
using MediatR;

namespace Application.Features.Comments.Queries.GetLessonComments;

public class GetLessonCommentsQuery : IRequest<Result<List<CommentDto>>>
{
    public int LessonId { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; } = 20;

    public GetLessonCommentsQuery(int lessonId, int pageIndex = 0, int pageSize = 20)
    {
        LessonId = lessonId;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
