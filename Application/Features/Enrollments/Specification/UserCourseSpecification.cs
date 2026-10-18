using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Enrollments.Specifications;

public class UserCourseSpecification
    : BaseSpecification<UserCourse>
{
    public UserCourseSpecification(
        Guid applicationUserId,
        int courseId)
    {
        Criteria = x =>
            x.ApplicationUserId == applicationUserId &&
            x.CourseId == courseId;
    }
}