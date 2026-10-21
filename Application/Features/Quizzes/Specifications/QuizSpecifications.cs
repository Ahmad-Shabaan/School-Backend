using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Quizzes.Specifications;

public class QuizWithQuestionsSpecification : BaseSpecification<Quiz>
{
    public QuizWithQuestionsSpecification(int quizId)
    {
        Criteria = q => q.Id == quizId;
        AddInclude("Questions");
        AddInclude("Questions.Options");
    }
}

public class QuizByLessonWithQuestionsSpecification : BaseSpecification<Quiz>
{
    public QuizByLessonWithQuestionsSpecification(int lessonId)
    {
        Criteria = q => q.LessonId == lessonId;
        AddInclude("Questions");
        AddInclude("Questions.Options");
    }
}

public class QuizAttemptsByStudentAndQuizSpecification
    : BaseSpecification<QuizAttempt>
{
    public QuizAttemptsByStudentAndQuizSpecification(
        Guid studentId,
        int quizId)
    {
        Criteria = a =>
            a.ApplicationUserId == studentId &&
            a.QuizId == quizId;

        ApplyOrderByDescending(a => a.AttemptNumber);
    }
}

public class QuizAttemptsByQuizSpecification
    : BaseSpecification<QuizAttempt>
{
    public QuizAttemptsByQuizSpecification(int quizId)
    {
        Criteria = a => a.QuizId == quizId;
    }
}

public class QuizzesByCourseSpecification : BaseSpecification<Quiz>
{
    public QuizzesByCourseSpecification(int courseId)
    {
        Criteria = q => q.Lesson.CourseId == courseId;
    }
}

public class QuizAttemptsByStudentAndCourseSpecification
    : BaseSpecification<QuizAttempt>
{
    public QuizAttemptsByStudentAndCourseSpecification(
        Guid studentId,
        int courseId)
    {
        Criteria = a =>
            a.ApplicationUserId == studentId &&
            a.Quiz.Lesson.CourseId == courseId;
    }
}

public class QuizAttemptsWithAnswersByStudentAndQuizSpecification
    : BaseSpecification<QuizAttempt>
{
    public QuizAttemptsWithAnswersByStudentAndQuizSpecification(
        Guid studentId,
        int quizId)
    {
        Criteria = a =>
            a.ApplicationUserId == studentId &&
            a.QuizId == quizId;

        AddInclude("Answers");

        ApplyOrderByDescending(a => a.AttemptNumber);
    }
}
