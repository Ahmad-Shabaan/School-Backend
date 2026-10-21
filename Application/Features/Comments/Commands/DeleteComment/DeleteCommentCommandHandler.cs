using Application.Common;
using Application.Features.Comments.Errors;
using Application.Features.Comments.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler
    : IRequestHandler<DeleteCommentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork
            .Repository<LessonComment>()
            .FirstOrDefaultAsync(
                new CommentByIdSpecification(request.Id),
                cancellationToken);

        if (comment is null || comment.IsDeleted)
            return Result.Failed(
                "Comment was not found.",
                CommentErrors.CommentNotFound);

        var isAuthor = comment.AuthorId == _currentUserService.UserId;
        var isStaff = _currentUserService.IsStaff();

        if (!isAuthor && !isStaff)
            return Result.Failed(
                "You are not authorized to delete this comment.",
                CommentErrors.NotCommentAuthor);

        var commentRepository =
            _unitOfWork.Repository<LessonComment>();

        comment.IsDeleted = true;
        commentRepository.Update(comment);

        if (comment.ParentCommentId is null)
        {
            var replies = await commentRepository.ListAsync(
                new RepliesByParentSpecification(comment.Id),
                cancellationToken);

            foreach (var reply in replies)
            {
                reply.IsDeleted = true;
                commentRepository.Update(reply);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Succeeded("Comment deleted successfully.");
    }
}
