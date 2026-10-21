using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Progress.Specifications;

public class LessonProgressByStudentAndCourseSpecification
    : BaseSpecification<Domain.Entities.LessonProgress>
{
    public LessonProgressByStudentAndCourseSpecification(
        Guid studentId,
        int courseId)
    {
        Criteria = p =>
            p.ApplicationUserId == studentId &&
            p.Lesson.CourseId == courseId;
    }
}

public class WatchSegmentsByStudentAndLessonSpecification
    : BaseSpecification<LessonWatchSegment>
{
    public WatchSegmentsByStudentAndLessonSpecification(
        Guid studentId,
        int lessonId)
    {
        Criteria = s =>
            s.ApplicationUserId == studentId &&
            s.LessonId == lessonId;

        ApplyOrderBy(s => s.StartSeconds);
    }
}
