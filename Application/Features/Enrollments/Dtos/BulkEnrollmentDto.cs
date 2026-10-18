namespace Application.Features.Enrollments.DTOs;

public class BulkEnrollmentDto
{
    public int CourseId { get; set; }

    public List<EnrollmentDto> Enrollments { get; set; }= [];

    public List<string> AlreadyEnrolledEmails { get; set; }= new();

    public List<string> NotFoundEmails { get; set; }= new();
}