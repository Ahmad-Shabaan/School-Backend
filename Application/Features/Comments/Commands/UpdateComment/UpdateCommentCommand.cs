using Application.Common;
using Application.Features.Comments.DTOs;
using MediatR;

namespace Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommand : IRequest<Result<CommentDto>>
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;
}
