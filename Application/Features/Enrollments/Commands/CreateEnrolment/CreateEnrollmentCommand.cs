using Application.Common;
using Application.Features.Enrollments.DTOs;
using MediatR;

namespace Application.Features.Enrollments.Commands.CreateBulkEnrollment;

public class CreateEnrollmentCommand
    : IRequest<Result<BulkEnrollmentDto>>
{
    public int CourseId { get; set; }

    public List<string> Emails { get; set; } = new();
}