namespace Application.Features.Lessons.DTOs;

public class LessonResourceInput
{
    public string Title { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? FileType { get; set; }
}
