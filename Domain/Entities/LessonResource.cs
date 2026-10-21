namespace Domain.Entities;

public class LessonResource
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? FileType { get; set; }
}
