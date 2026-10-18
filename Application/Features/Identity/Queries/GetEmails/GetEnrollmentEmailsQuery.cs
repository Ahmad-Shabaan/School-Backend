using Application.Common;
using Application.Features.Enrollments.DTOs;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Queries.GetEmails
{
    public sealed record GetEnrollmentEmailsQuery(int CourseId) : IRequest<Result<IReadOnlyList<EnrollmentEmailsDto>>>
    {
    }
}
