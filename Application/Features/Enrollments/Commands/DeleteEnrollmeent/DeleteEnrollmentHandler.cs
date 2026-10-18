using Application.Common;
using Application.Features.Enrollments.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment;

public class DeleteEnrollmentCommandHandler
    : IRequestHandler<
        DeleteEnrollmentCommand,
        Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEnrollmentCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        DeleteEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StudentEmail))
        {
            return Result<bool>.Failed(
                "Student email is required");
        }

        var courseRepository =
            _unitOfWork.Repository<Course>();

        var course = await courseRepository.GetByIdAsync(
            request.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result<bool>.Failed(
                "Course not found");
        }

        var userRepository =
            _unitOfWork.Repository<ApplicationUser>();

        var usersSpecification =
            new UsersByEmailsSpecification(
                new[] { request.StudentEmail });

        var users = await userRepository.ListAsync(
            usersSpecification,
            cancellationToken);

        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result<bool>.Failed(
                "Student not found");
        }

        var userCourseRepository =
            _unitOfWork.Repository<UserCourse>();

        var specification =
            new UserCourseSpecification(
                user.Id,
                request.CourseId);

        var enrollment =
            await userCourseRepository.FirstOrDefaultAsync(
                specification,
                cancellationToken);

        if (enrollment is null)
        {
            return Result<bool>.Failed(
                "Enrollment not found");
        }

        userCourseRepository.Delete(enrollment);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<bool>.Succeeded(
            true,
            "Enrollment deleted successfully");
    }
}