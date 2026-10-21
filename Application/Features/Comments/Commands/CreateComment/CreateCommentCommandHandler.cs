using Application.Common;
using Application.Features.Comments.DTOs;
using Application.Features.Comments.Errors;
using Application.Features.Comments.Specifications;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler
    : IRequestHandler<CreateCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CommentDto>> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var content = request.Content?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
            return Result<CommentDto>.Failed(
                "Comment content cannot be empty.",
                CommentErrors.EmptyContent);

        var studentId = _currentUserService.UserId;

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<CommentDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            studentId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<CommentDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        LessonComment? parentComment = null;

        if (request.ParentCommentId.HasValue)
        {
            parentComment = await _unitOfWork
                .Repository<LessonComment>()
                .FirstOrDefaultAsync(
                    new CommentByIdSpecification(
                        request.ParentCommentId.Value),
                    cancellationToken);

            if (parentComment is null ||
                parentComment.LessonId != request.LessonId ||
                parentComment.IsDeleted)
            {
                return Result<CommentDto>.Failed(
                    "The parent comment does not belong to this lesson.",
                    CommentErrors.InvalidParent);
            }

            if (parentComment.ParentCommentId.HasValue)
            {
                return Result<CommentDto>.Failed(
                    "Replies can only be added to top-level comments.",
                    CommentErrors.ReplyNotAllowed);
            }
        }

        var comment = new LessonComment
        {
            LessonId = request.LessonId,
            AuthorId = studentId,
            Content = content,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork
            .Repository<LessonComment>()
            .AddAsync(comment, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var author = await _unitOfWork
            .Repository<ApplicationUser>()
            .GetByIdAsync(studentId, cancellationToken);

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
            "Comment created successfully.");
    }
}
