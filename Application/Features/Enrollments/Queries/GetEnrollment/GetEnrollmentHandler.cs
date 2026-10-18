using Application.Common;
using Application.Features.Enrollments.DTOs;
using Application.Features.Enrollments.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetCourseEnrollments;

public class GetCourseEnrollmentsQueryHandler
    : IRequestHandler<
        GetEnrollmentsQuery,
        Result<IReadOnlyList<EnrollmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCourseEnrollmentsQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<EnrollmentDto>>> Handle(
        GetEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Check course
        var courseRepository =
            _unitOfWork.Repository<Course>();

        var course = await courseRepository.GetByIdAsync(
            request.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result<IReadOnlyList<EnrollmentDto>>.Failed(
                "Course not found");
        }

        // Get enrollments
        var repository =
            _unitOfWork.Repository<UserCourse>();

        var specification =
            new UserCoursesSpecification(request.CourseId);

        var enrollments = await repository.ListAsync(
            specification,
            cancellationToken);

        var result = enrollments
            .Select(x => new EnrollmentDto
            {
                ApplicationUserId = x.ApplicationUserId,
                Email = x.ApplicationUser?.Email ?? string.Empty,
                CourseId = x.CourseId
            })
            .ToList();

        return Result<IReadOnlyList<EnrollmentDto>>.Succeeded(
            result,
            "Course enrollments retrieved successfully");
    }
}