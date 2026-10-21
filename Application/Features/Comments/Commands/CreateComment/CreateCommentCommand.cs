using Application.Common;
using Application.Features.Comments.DTOs;
using MediatR;

namespace Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommand : IRequest<Result<CommentDto>>
{
    public int LessonId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }
}
