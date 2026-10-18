using Application.Common;
using Application.Features.Enrollments.DTOs;
using Application.Features.Enrollments.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Enrollments.Commands.UpdateEnrollment;

public class UpdateEnrollmentCommandHandler
    : IRequestHandler<
        UpdateEnrollmentCommand,
        Result<EnrollmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEnrollmentCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EnrollmentDto>> Handle(
        UpdateEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OldStudentEmail) ||
            string.IsNullOrWhiteSpace(request.NewStudentEmail))
        {
            return Result<EnrollmentDto>.Failed(
                "Old and new student emails are required");
        }

        var oldEmail =
            request.OldStudentEmail.Trim().ToLower();

        var newEmail =
            request.NewStudentEmail.Trim().ToLower();

        if (oldEmail == newEmail)
        {
            return Result<EnrollmentDto>.Failed(
                "Old and new student emails cannot be the same");
        }

        var courseRepository =
            _unitOfWork.Repository<Course>();

        var course = await courseRepository.GetByIdAsync(
            request.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result<EnrollmentDto>.Failed(
                "Course not found");
        }

        
        var userRepository =
            _unitOfWork.Repository<ApplicationUser>();

        var usersSpecification =
            new UsersByEmailsSpecification(
                new[] { oldEmail, newEmail });

        var users = await userRepository.ListAsync(
            usersSpecification,
            cancellationToken);

        var oldUser = users.FirstOrDefault(
            x => x.Email != null &&
                 x.Email.ToLower() == oldEmail);

        if (oldUser is null)
        {
            return Result<EnrollmentDto>.Failed(
                "Old student not found");
        }

        var newUser = users.FirstOrDefault(
            x => x.Email != null &&
                 x.Email.ToLower() == newEmail);

        if (newUser is null)
        {
            return Result<EnrollmentDto>.Failed(
                "New student not found");
        }

        var userCourseRepository =
            _unitOfWork.Repository<UserCourse>();

        var oldEnrollmentSpecification =
            new UserCourseSpecification(
                oldUser.Id,
                request.CourseId);

        var enrollment =
            await userCourseRepository.FirstOrDefaultAsync(
                oldEnrollmentSpecification,
                cancellationToken);

        if (enrollment is null)
        {
            return Result<EnrollmentDto>.Failed(
                "Enrollment not found");
        }

        var newEnrollmentSpecification =
            new UserCourseSpecification(
                newUser.Id,
                request.CourseId);

        var existingNewEnrollment =
            await userCourseRepository.FirstOrDefaultAsync(
                newEnrollmentSpecification,
                cancellationToken);

        if (existingNewEnrollment is not null)
        {
            return Result<EnrollmentDto>.Failed(
                "New student is already enrolled in this course");
        }

        
        enrollment.ApplicationUserId = newUser.Id;

        userCourseRepository.Update(enrollment);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var result = new EnrollmentDto
        {
            ApplicationUserId = newUser.Id,
            Email = newUser.Email ?? string.Empty,
            CourseId = request.CourseId
        };

        return Result<EnrollmentDto>.Succeeded(
            result,
            "Enrollment updated successfully");
    }
}