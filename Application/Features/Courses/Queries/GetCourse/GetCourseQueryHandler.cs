using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Errors;
using Application.Features.Courses.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourse;

public class GetCourseQueryHandler
    : IRequestHandler<GetCourseQuery, Result<CourseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCourseQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CourseDto>> Handle(
        GetCourseQuery request,
        CancellationToken cancellationToken)
    {
        var specification =
            new CourseByIdSpecification(request.Id);

        var course = await _unitOfWork
            .Repository<Course>()
            .FirstOrDefaultAsync(
                specification,
                cancellationToken);

        if (course is null)
            return Result<CourseDto>.Failed(
                "Course is not found.",
                CourseErrors.CourseNotFound);

        var response = new CourseDto
        {
            Id = course.Id,
            CourseName = course.CourseName,
            Description = course.Description,
            File = course.File
        };

        return Result<CourseDto>.Succeeded(response);
    }
}