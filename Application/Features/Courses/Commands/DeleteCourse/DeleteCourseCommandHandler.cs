using Application.Common;
using Application.Features.Courses.Errors;
using Application.Features.Courses.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseCommandHandler
    : IRequestHandler<DeleteCourseCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        DeleteCourseCommand request,
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
            return Result<bool>.Failed(
                "Course is not found.",
                CourseErrors.CourseNotFound);

        _unitOfWork
            .Repository<Course>()
            .Delete(course);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<bool>.Succeeded(
            true,
            "Course deleted successfully.");
    }
}