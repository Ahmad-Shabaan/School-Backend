using Application.Common;
using System.Net;

namespace Application.Features.Comments.Errors;

public static class CommentErrors
{
    public static readonly Error CommentNotFound = new("Comment.NotFound", "Comment was not found.", HttpStatusCode.NotFound);
    public static readonly Error NotCommentAuthor = new("Comment.NotAuthor", "Only the comment author can perform this action.", HttpStatusCode.Forbidden);
    public static readonly Error InvalidParent = new("Comment.InvalidParent", "The parent comment does not belong to this lesson.", HttpStatusCode.BadRequest);
    public static readonly Error ReplyNotAllowed = new("Comment.ReplyNotAllowed", "Replies can only be added to top-level comments.", HttpStatusCode.BadRequest);
    public static readonly Error EmptyContent = new("Comment.EmptyContent", "Comment content cannot be empty.", HttpStatusCode.BadRequest);
}
