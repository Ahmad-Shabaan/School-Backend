using Application.Common;
using Application.Features.Enrollments.DTOs;
using Application.Features.Enrollments.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Enrollments.Commands.CreateBulkEnrollment;

public class CreateBulkEnrollmentCommandHandler
    : IRequestHandler<
        CreateEnrollmentCommand,
        Result<BulkEnrollmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBulkEnrollmentCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BulkEnrollmentDto>> Handle(
        CreateEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var courseRepository = _unitOfWork.Repository<Course>();

        var course = await courseRepository.GetByIdAsync(
            request.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result<BulkEnrollmentDto>.Failed(
                "Course not found");
        }

        if (request.Emails is null || request.Emails.Count == 0)
        {
            return Result<BulkEnrollmentDto>.Failed(
                "At least one email is required");
        }

        var emails = request.Emails
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLower())
            .Distinct()
            .ToList();

        if (emails.Count == 0)
        {
            return Result<BulkEnrollmentDto>.Failed(
                "No valid emails were provided");
        }

        var userRepository =
            _unitOfWork.Repository<ApplicationUser>();

        var usersSpecification =
            new UsersByEmailsSpecification(emails);

        var users = await userRepository.ListAsync(
            usersSpecification,
            cancellationToken);

        var foundEmails = users
            .Where(x => x.Email != null)
            .Select(x => x.Email!.Trim().ToLower())
            .ToHashSet();

        var notFoundEmails = emails
            .Where(email => !foundEmails.Contains(email))
            .ToList();

        var userCourseRepository =
            _unitOfWork.Repository<UserCourse>();

        var existingSpecification =
            new UserCoursesSpecification(request.CourseId);

        var existingEnrollments =
            await userCourseRepository.ListAsync(
                existingSpecification,
                cancellationToken);

        var alreadyEnrolledUserIds = existingEnrollments
            .Select(x => x.ApplicationUserId)
            .ToHashSet();

        var response = new BulkEnrollmentDto
        {
            CourseId = request.CourseId,
            NotFoundEmails = notFoundEmails
        };

        foreach (var user in users)
        {
            if (alreadyEnrolledUserIds.Contains(user.Id))
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    response.AlreadyEnrolledEmails.Add(
                        user.Email);
                }

                continue;
            }

            var enrollment = new UserCourse
            {
                ApplicationUserId = user.Id,
                CourseId = request.CourseId
            };

            userCourseRepository.Add(enrollment);
            response.Enrollments.Add(
                new EnrollmentDto
                {
                    ApplicationUserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    CourseId = request.CourseId
                });
        }

        if (response.Enrollments.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        return Result<BulkEnrollmentDto>.Succeeded(
            response,
            "Enrollment process completed successfully");
    }
}