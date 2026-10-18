using Application.Common;
using Application.Features.Enrollments.DTOs;
using MediatR;

namespace Application.Features.Enrollments.Commands.UpdateEnrollment;

public class UpdateEnrollmentCommand
    : IRequest<Result<EnrollmentDto>>
{
    public int CourseId { get; set; }

    public string OldStudentEmail { get; set; } = string.Empty;

    public string NewStudentEmail { get; set; } = string.Empty;
}