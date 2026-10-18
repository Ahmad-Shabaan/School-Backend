using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommand : IRequest<Result<CourseDto>>
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }

    public string Email { get; set; } = string.Empty;
}