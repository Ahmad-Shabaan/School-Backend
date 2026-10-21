using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Lessons.Specifications;

public class LessonByIdWithCourseSpecification : BaseSpecification<Lesson>
{
    public LessonByIdWithCourseSpecification(int lessonId)
    {
        Criteria = l => l.Id == lessonId;
        AddInclude(l => l.Course);
    }
}

public class UserCourseByStudentAndCourseSpecification
    : BaseSpecification<UserCourse>
{
    public UserCourseByStudentAndCourseSpecification(
        Guid studentId,
        int courseId)
    {
        Criteria = uc =>
            uc.ApplicationUserId == studentId &&
            uc.CourseId == courseId;
    }
}

public class PreviousLessonSpecification : BaseSpecification<Lesson>
{
    public PreviousLessonSpecification(int courseId, int currentOrder)
    {
        Criteria = l =>
            l.CourseId == courseId &&
            l.Order < currentOrder;

        ApplyOrderByDescending(l => l.Order);
    }
}

public class LessonProgressByStudentAndLessonSpecification
    : BaseSpecification<LessonProgress>
{
    public LessonProgressByStudentAndLessonSpecification(
        Guid studentId,
        int lessonId)
    {
        Criteria = p =>
            p.ApplicationUserId == studentId &&
            p.LessonId == lessonId;
    }
}

public class QuizByLessonSpecification : BaseSpecification<Quiz>
{
    public QuizByLessonSpecification(int lessonId)
    {
        Criteria = q => q.LessonId == lessonId;
    }
}

public class PassedQuizAttemptSpecification : BaseSpecification<QuizAttempt>
{
    public PassedQuizAttemptSpecification(Guid studentId, int quizId)
    {
        Criteria = a =>
            a.ApplicationUserId == studentId &&
            a.QuizId == quizId &&
            a.IsPassed &&
            a.Score >= a.Quiz.PassingScorePercent;

        ApplyOrderByDescending(a => a.Score);
    }
}
