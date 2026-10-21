using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Progress.DTOs;
using Application.Features.Progress.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Progress.Queries.GetLessonProgress;

public class GetLessonProgressQueryHandler
    : IRequestHandler<GetLessonProgressQuery, Result<LessonProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonProgressQueryHandler(
        IUnitOfWork unitOfWork,
        ILessonAccessService lessonAccessService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _lessonAccessService = lessonAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonProgressDto>> Handle(
        GetLessonProgressQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;

        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .GetByIdAsync(request.LessonId, cancellationToken);

        if (lesson is null)
            return Result<LessonProgressDto>.Failed(
                "Lesson was not found.",
                LessonErrors.LessonNotFound);

        var access = await _lessonAccessService.CheckAccessAsync(
            request.LessonId,
            studentId,
            cancellationToken);

        if (!access.IsAccessible)
            return Result<LessonProgressDto>.Failed(
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                    ? "You are not enrolled in this course."
                    : "The lesson is not unlocked yet.",
                access.MissingRequirement ==
                    LessonAccessRequirements.Enrollment
                ? LessonErrors.NotEnrolled
                : LessonErrors.LessonLocked);

        var progress = await _unitOfWork
            .Repository<Domain.Entities.LessonProgress>()
            .FirstOrDefaultAsync(
                new LessonProgressByStudentAndLessonSpecification(
                    studentId,
                    request.LessonId),
                cancellationToken);

        var response = progress is null
            ? new LessonProgressDto
            {
                LessonId = request.LessonId
            }
            : new LessonProgressDto
            {
                LessonId = progress.LessonId,
                LastPositionSeconds = progress.LastPositionSeconds,
                DurationSeconds = progress.DurationSeconds,
                WatchedPercentage = progress.WatchedPercentage,
                IsVideoCompleted = progress.IsWatched,
                UpdatedAt = progress.UpdatedAt
            };

        return Result<LessonProgressDto>.Succeeded(response);
    }
}
