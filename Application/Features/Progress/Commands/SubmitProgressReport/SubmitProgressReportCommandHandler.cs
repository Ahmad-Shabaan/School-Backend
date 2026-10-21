using Application.Common;
using Application.Features.Lessons.Errors;
using Application.Features.Lessons.Specifications;
using Application.Features.Progress.DTOs;
using Application.Features.Progress.Errors;
using Application.Features.Progress.Specifications;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Progress.Commands.SubmitProgressReport;

public class SubmitProgressReportCommandHandler
    : IRequestHandler<SubmitProgressReportCommand, Result<LessonProgressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILessonAccessService _lessonAccessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly LearningPlatformOptions _options;

    public SubmitProgressReportCommandHandler(
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

    public async Task<Result<LessonProgressDto>> Handle(
        SubmitProgressReportCommand request,
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

        if (string.IsNullOrWhiteSpace(lesson.YouTubeVideoId))
            return Result<LessonProgressDto>.Failed(
                "The lesson has no video to track.",
                ProgressErrors.LessonHasNoVideo);

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

        var positionTolerance = _options.ProgressPositionToleranceSeconds;

        if (request.DurationSeconds <= 0 ||
            request.DurationSeconds > _options.MaxVideoDurationSeconds)
        {
            return Result<LessonProgressDto>.Failed(
                "The reported video duration is invalid.",
                ProgressErrors.InvalidDuration);
        }

        if (request.CurrentTimeSeconds < 0 ||
            request.CurrentTimeSeconds >
                request.DurationSeconds + positionTolerance)
        {
            return Result<LessonProgressDto>.Failed(
                "The reported playback position is invalid.",
                ProgressErrors.InvalidPosition);
        }

        var hasSegment = request.WatchedSegmentStartSeconds.HasValue;

        if (hasSegment)
        {
            var segmentStart = request.WatchedSegmentStartSeconds!.Value;
            var segmentEnd = request.WatchedSegmentEndSeconds!.Value;

            if (segmentStart < 0 ||
                segmentEnd <= segmentStart ||
                segmentEnd > request.DurationSeconds + positionTolerance ||
                segmentEnd > request.CurrentTimeSeconds + positionTolerance)
            {
                return Result<LessonProgressDto>.Failed(
                    "The reported watch segment is invalid.",
                    ProgressErrors.InvalidSegment);
            }

            if (segmentEnd - segmentStart >
                _options.MaxWatchSegmentSeconds)
            {
                return Result<LessonProgressDto>.Failed(
                    "The reported watch segment is too long; report progress periodically instead.",
                    ProgressErrors.SegmentTooLong);
            }
        }

        var segments = await _unitOfWork
            .Repository<LessonWatchSegment>()
            .ListAsync(
                new WatchSegmentsByStudentAndLessonSpecification(
                    studentId,
                    request.LessonId),
                cancellationToken);

        if (hasSegment)
        {
            var newStart = request.WatchedSegmentStartSeconds!.Value;
            var newEnd = request.WatchedSegmentEndSeconds!.Value;

            var isAlreadyCovered = segments.Any(s =>
                s.StartSeconds <= newStart &&
                s.EndSeconds >= newEnd);

            if (!isAlreadyCovered)
            {
                await _unitOfWork
                    .Repository<LessonWatchSegment>()
                    .AddAsync(
                        new LessonWatchSegment
                        {
                            ApplicationUserId = studentId,
                            LessonId = request.LessonId,
                            StartSeconds = newStart,
                            EndSeconds = newEnd
                        },
                        cancellationToken);
            }
        }

        var intervals = segments
            .Select(s => new WatchCoverageCalculator.Interval(
                s.StartSeconds,
                s.EndSeconds))
            .ToList();

        if (hasSegment)
        {
            intervals.Add(new WatchCoverageCalculator.Interval(
                request.WatchedSegmentStartSeconds!.Value,
                request.WatchedSegmentEndSeconds!.Value));
        }

        var watchedPercentage =
            WatchCoverageCalculator.CalculateWatchedPercentage(
                intervals,
                request.DurationSeconds);

        var isVideoCompleted = watchedPercentage >=
                               _options.VideoCompletionThresholdPercent;

        var progress = await _unitOfWork
            .Repository<Domain.Entities.LessonProgress>()
            .FirstOrDefaultAsync(
                new LessonProgressByStudentAndLessonSpecification(
                    studentId,
                    request.LessonId),
                cancellationToken);

        if (progress is null)
        {
            progress = new Domain.Entities.LessonProgress
            {
                ApplicationUserId = studentId,
                LessonId = request.LessonId
            };

            await _unitOfWork
                .Repository<Domain.Entities.LessonProgress>()
                .AddAsync(progress, cancellationToken);
        }

        progress.LastPositionSeconds = request.CurrentTimeSeconds;
        progress.DurationSeconds = request.DurationSeconds;
        progress.WatchedPercentage = watchedPercentage;
        progress.IsWatched = isVideoCompleted;
        progress.WatchedAt = isVideoCompleted
            ? progress.WatchedAt ?? DateTime.UtcNow
            : null;
        progress.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LessonProgressDto>.Succeeded(
            new LessonProgressDto
            {
                LessonId = progress.LessonId,
                LastPositionSeconds = progress.LastPositionSeconds,
                DurationSeconds = progress.DurationSeconds,
                WatchedPercentage = progress.WatchedPercentage,
                IsVideoCompleted = progress.IsWatched,
                UpdatedAt = progress.UpdatedAt
            },
            "Progress updated successfully.");
    }
}
