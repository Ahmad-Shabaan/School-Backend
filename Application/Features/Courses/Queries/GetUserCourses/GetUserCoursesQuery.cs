using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;

namespace Application.Features.Courses.Queries.GetUserCourses;

public class GetUserCoursesQuery : IRequest<Result<List<CourseDto>>>
{
    public string Email { get; set; } = string.Empty;

    public GetUserCoursesQuery(string email)
    {
        Email = email;
    }
}