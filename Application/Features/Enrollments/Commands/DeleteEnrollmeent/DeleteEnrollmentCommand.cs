using Application.Common;
using MediatR;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment;

public class DeleteEnrollmentCommand
    : IRequest<Result<bool>>
{
    public int CourseId { get; set; }

    public string StudentEmail { get; set; } = string.Empty;
}