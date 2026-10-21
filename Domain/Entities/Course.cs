namespace Domain.Entities;

public class Course
{
    public int Id { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte[]? File { get; set; }

    public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}