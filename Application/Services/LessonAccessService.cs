using Application.Common;
using Application.Features.Lessons.Specifications;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class LessonAccessService : ILessonAccessService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly LearningPlatformOptions _options;
    private readonly ICurrentUserService _currentUserService;

    public LessonAccessService(
        IUnitOfWork unitOfWork,
        IOptions<LearningPlatformOptions> options,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _currentUserService = currentUserService;
    }

    public async Task<LessonAccessResult> CheckAccessAsync(
        int lessonId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await _unitOfWork
            .Repository<Lesson>()
            .FirstOrDefaultAsync(
                new LessonByIdWithCourseSpecification(lessonId),
                cancellationToken);

        if (lesson is null)
        {
            return new LessonAccessResult(
                IsAccessible: false,
                IsEnrolled: false,
                IsVideoCompleted: false,
                IsQuizPassed: false,
                MissingRequirement: LessonAccessRequirements.LessonNotFound,
                LessonId: lessonId,
                CourseId: 0,
                LessonOrder: 0);
        }

        if (_currentUserService.IsStaff())
        {
            return new LessonAccessResult(
                IsAccessible: true,
                IsEnrolled: true,
                IsVideoCompleted: true,
                IsQuizPassed: true,
                MissingRequirement: null,
                LessonId: lesson.Id,
                CourseId: lesson.CourseId,
                LessonOrder: lesson.Order);
        }

        var enrollment = await _unitOfWork
            .Repository<UserCourse>()
            .FirstOrDefaultAsync(
                new UserCourseByStudentAndCourseSpecification(
                    studentId,
                    lesson.CourseId),
                cancellationToken);

        var isEnrolled = enrollment is not null;

        if (!isEnrolled)
        {
            return new LessonAccessResult(
                IsAccessible: false,
                IsEnrolled: false,
                IsVideoCompleted: false,
                IsQuizPassed: false,
                MissingRequirement: LessonAccessRequirements.Enrollment,
                LessonId: lesson.Id,
                CourseId: lesson.CourseId,
                LessonOrder: lesson.Order);
        }

        if (lesson.Order <= 1)
        {
            return new LessonAccessResult(
                IsAccessible: true,
                IsEnrolled: true,
                IsVideoCompleted: true,
                IsQuizPassed: true,
                MissingRequirement: null,
                LessonId: lesson.Id,
                CourseId: lesson.CourseId,
                LessonOrder: lesson.Order);
        }

        var previousLesson = await _unitOfWork
            .Repository<Lesson>()
            .FirstOrDefaultAsync(
                new PreviousLessonSpecification(
                    lesson.CourseId,
                    lesson.Order),
                cancellationToken);

        if (previousLesson is null)
        {
            return new LessonAccessResult(
                IsAccessible: true,
                IsEnrolled: true,
                IsVideoCompleted: true,
                IsQuizPassed: true,
                MissingRequirement: null,
                LessonId: lesson.Id,
                CourseId: lesson.CourseId,
                LessonOrder: lesson.Order);
        }

        var isVideoCompleted = await IsPreviousLessonVideoCompletedAsync(
            previousLesson,
            studentId,
            cancellationToken);

        var isQuizPassed = await IsPreviousLessonQuizPassedAsync(
            previousLesson,
            studentId,
            cancellationToken);

        var isAccessible = isVideoCompleted && isQuizPassed;

        string? missingRequirement = null;

        if (!isAccessible)
        {
            missingRequirement = !isVideoCompleted
                ? LessonAccessRequirements.Video
                : LessonAccessRequirements.Quiz;
        }

        return new LessonAccessResult(
            IsAccessible: isAccessible,
            IsEnrolled: true,
            IsVideoCompleted: isVideoCompleted,
            IsQuizPassed: isQuizPassed,
            MissingRequirement: missingRequirement,
            LessonId: lesson.Id,
            CourseId: lesson.CourseId,
            LessonOrder: lesson.Order);
    }

    private async Task<bool> IsPreviousLessonVideoCompletedAsync(
        Lesson previousLesson,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(previousLesson.YouTubeVideoId))
            return true;

        var progress = await _unitOfWork
            .Repository<LessonProgress>()
            .FirstOrDefaultAsync(
                new LessonProgressByStudentAndLessonSpecification(
                    studentId,
                    previousLesson.Id),
                cancellationToken);

        if (progress is null || progress.DurationSeconds <= 0)
            return false;

        return progress.WatchedPercentage >=
               _options.VideoCompletionThresholdPercent;
    }

    private async Task<bool> IsPreviousLessonQuizPassedAsync(
        Lesson previousLesson,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var quiz = await _unitOfWork
            .Repository<Quiz>()
            .FirstOrDefaultAsync(
                new QuizByLessonSpecification(previousLesson.Id),
                cancellationToken);

        if (quiz is null)
            return true;

        var passedAttempt = await _unitOfWork
            .Repository<QuizAttempt>()
            .FirstOrDefaultAsync(
                new PassedQuizAttemptSpecification(
                    studentId,
                    quiz.Id),
                cancellationToken);

        return passedAttempt is not null;
    }
}
