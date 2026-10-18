using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Courses.Specifications;

public class CourseByIdSpecification : BaseSpecification<Course>
{
    public CourseByIdSpecification(int id)
    {
        Criteria = c => c.Id == id;
    }
}