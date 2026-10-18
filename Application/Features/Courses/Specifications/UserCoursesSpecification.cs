using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Courses.Specifications;

public class UserCoursesSpecification : BaseSpecification<UserCourse>
{
    public UserCoursesSpecification(Guid userId)
    {
        Criteria = uc => uc.ApplicationUserId == userId;

        AddInclude(uc => uc.Course);
    }
}