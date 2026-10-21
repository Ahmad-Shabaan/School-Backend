using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Lessons.Specifications;

public class LessonsByCourseIdSpecification : BaseSpecification<Lesson>
{
    public LessonsByCourseIdSpecification(int courseId)
    {
        Criteria = l => l.CourseId == courseId;
        ApplyOrderBy(l => l.Order);
    }
}

public class LessonByIdSpecification : BaseSpecification<Lesson>
{
    public LessonByIdSpecification(int lessonId)
    {
        Criteria = l => l.Id == lessonId;
    }
}

public class LessonWithDetailsSpecification : BaseSpecification<Lesson>
{
    public LessonWithDetailsSpecification(int lessonId)
    {
        Criteria = l => l.Id == lessonId;
        AddInclude(l => l.Goals);
        AddInclude(l => l.Resources);
        AddInclude(l => l.Quiz!);
    }
}

public class LessonWithDetailsByCourseSpecification : BaseSpecification<Lesson>
{
    public LessonWithDetailsByCourseSpecification(int courseId)
    {
        Criteria = l => l.CourseId == courseId;
        AddInclude(l => l.Goals);
        AddInclude(l => l.Resources);
        ApplyOrderBy(l => l.Order);
    }
}

public class LessonsByCourseWithProgressSpecification : BaseSpecification<Lesson>
{
    public LessonsByCourseWithProgressSpecification(int courseId)
    {
        Criteria = l => l.CourseId == courseId;
        ApplyOrderBy(l => l.Order);
    }
}
