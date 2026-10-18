using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Errors;
using Application.Features.Courses.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler
    : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CourseDto>> Handle(
        UpdateCourseCommand request,
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

        course.CourseName = request.Name;
        course.Description = request.Description;

        if (request.Photo is not null)
        {
            using var dataStream = new MemoryStream();

            await request.Photo.CopyToAsync(
                dataStream,
                cancellationToken);

            course.File = dataStream.ToArray();
        }

        _unitOfWork
            .Repository<Course>()
            .Update(course);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var response = new CourseDto
        {
            Id = course.Id,
            CourseName = course.CourseName,
            Description = course.Description,
            File = course.File
        };

        return Result<CourseDto>.Succeeded(
            response,
            "Course updated successfully.");
    }
}