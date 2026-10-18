using Application.Common;
using Application.Features.Enrollments.DTOs;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetCourseEnrollments;

public class GetEnrollmentsQuery
    : IRequest<Result<IReadOnlyList<EnrollmentDto>>>
{
    public int CourseId { get; set; }

    public GetEnrollmentsQuery(int courseId)
    {
        CourseId = courseId;
    }
}