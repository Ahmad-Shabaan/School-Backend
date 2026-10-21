using Application.Common;
using Application.Features.Comments.DTOs;
using Application.Features.Comments.Errors;
using Application.Features.Comments.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler
    : IRequestHandler<UpdateCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CommentDto>> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var content = request.Content?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
            return Result<CommentDto>.Failed(
                "Comment content cannot be empty.",
                CommentErrors.EmptyContent);

        var comment = await _unitOfWork
            .Repository<LessonComment>()
            .FirstOrDefaultAsync(
                new CommentByIdSpecification(request.Id),
                cancellationToken);

        if (comment is null || comment.IsDeleted)
            return Result<CommentDto>.Failed(
                "Comment was not found.",
                CommentErrors.CommentNotFound);

        if (comment.AuthorId != _currentUserService.UserId)
            return Result<CommentDto>.Failed(
                "Only the comment author can edit this comment.",
                CommentErrors.NotCommentAuthor);

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork
            .Repository<LessonComment>()
            .Update(comment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var author = await _unitOfWork
            .Repository<ApplicationUser>()
            .GetByIdAsync(comment.AuthorId, cancellationToken);

        var response = new CommentDto
        {
            Id = comment.Id,
            LessonId = comment.LessonId,
            ParentCommentId = comment.ParentCommentId,
            AuthorId = comment.AuthorId,
            AuthorName = author is null
                ? string.Empty
                : $"{author.FirstName} {author.LastName}".Trim(),
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        return Result<CommentDto>.Succeeded(
            response,
            "Comment updated successfully.");
    }
}
