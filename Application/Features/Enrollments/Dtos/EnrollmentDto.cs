namespace Application.Features.Enrollments.DTOs;

public class EnrollmentDto
{
    public Guid ApplicationUserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public int CourseId { get; set; }
}