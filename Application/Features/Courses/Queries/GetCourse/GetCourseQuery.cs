using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourse;

public class GetCourseQuery : IRequest<Result<CourseDto>>
{
    public int Id { get; set; }

    public GetCourseQuery(int id)
    {
        Id = id;
    }
}