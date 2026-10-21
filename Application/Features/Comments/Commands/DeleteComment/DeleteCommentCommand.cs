using Application.Common;
using MediatR;

namespace Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommand : IRequest<Result>
{
    public int Id { get; set; }

    public DeleteCommentCommand(int id)
    {
        Id = id;
    }
}
