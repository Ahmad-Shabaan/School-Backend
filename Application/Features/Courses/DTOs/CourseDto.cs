namespace Application.Features.Courses.DTOs;

public class CourseDto
{
    public int Id { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte[]? File { get; set; }
}