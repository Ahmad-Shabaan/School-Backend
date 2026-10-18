using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourses
{
    public sealed record GetCoursesQuery : IRequest<Result<IEnumerable<CourseDto>>>
    {
    }
}
