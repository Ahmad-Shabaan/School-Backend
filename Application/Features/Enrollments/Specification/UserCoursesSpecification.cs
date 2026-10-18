using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Enrollments.Specifications;

public class UserCoursesSpecification
    : BaseSpecification<UserCourse>
{
    public UserCoursesSpecification(int courseId)
    {
        Criteria = x => x.CourseId == courseId;

        AddInclude(x => x.ApplicationUser);
    }
}