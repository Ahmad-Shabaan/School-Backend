using Application.Common;
using Application.Features.Comments.DTOs;
using Application.Features.Comments.Specifications;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Comments.Queries.GetLessonComments;

public class GetLessonCommentsQueryHandler
    : IRequestHandler<GetLessonCommentsQuery, Result<List<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly LearningPlatformOptions _options;

    public GetLessonCommentsQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService,
        IOptions<LearningPlatformOptions> options)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
        _options = options.Value;
    }

    public async Task<Result<List<CommentDto>>> Handle(
        GetLessonCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<List<CommentDto>>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            _currentUserService.UserId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<List<CommentDto>>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var pageSize = Math.Clamp(
            request.PageSize,
            1,
            _options.MaxCommentsPageSize);

        var pageIndex = Math.Max(0, request.PageIndex);

        var totalCount = await _unitOfWork
            .Repository<LessonComment>()
            .CountAsync(
                new CommentsByLessonSpecification(request.LessonId),
                cancellationToken);

        var pagedSpecification =
            new CommentsByLessonPagedSpecification(
                request.LessonId,
                pageIndex * pageSize,
                pageSize);

        var comments = await _unitOfWork
            .Repository<LessonComment>()
            .ListAsync(pagedSpecification, cancellationToken);

        var response = comments.Select(MapComment).ToList();

        var result = Result<List<CommentDto>>.Succeeded(response);

        result.Pagination = new Pagination
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        };

        return result;
    }

    internal static CommentDto MapComment(LessonComment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            LessonId = comment.LessonId,
            ParentCommentId = comment.ParentCommentId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author is null
                ? string.Empty
                : $"{comment.Author.FirstName} {comment.Author.LastName}".Trim(),
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Replies = comment.Replies
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new CommentDto
                {
                    Id = r.Id,
                    LessonId = r.LessonId,
                    ParentCommentId = r.ParentCommentId,
                    AuthorId = r.AuthorId,
                    AuthorName = r.Author is null
                        ? string.Empty
                        : $"{r.Author.FirstName} {r.Author.LastName}".Trim(),
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToList()
        };
    }
}
