using Application.Common;

namespace Application.Services;

public sealed record LessonAccessResult(
    bool IsAccessible,
    bool IsEnrolled,
    bool IsVideoCompleted,
    bool IsQuizPassed,
    string? MissingRequirement,
    int LessonId,
    int CourseId,
    int LessonOrder);

public interface ILessonAccessService
{
    Task<LessonAccessResult> CheckAccessAsync(
        int lessonId,
        Guid studentId,
        CancellationToken cancellationToken = default);
}
