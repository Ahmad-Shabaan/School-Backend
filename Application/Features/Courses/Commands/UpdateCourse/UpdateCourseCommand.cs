using Application.Common;
using Application.Features.Courses.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommand : IRequest<Result<CourseDto>>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }
}