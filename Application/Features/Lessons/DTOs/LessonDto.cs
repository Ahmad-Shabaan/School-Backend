namespace Application.Features.Lessons.DTOs;

public class LessonDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public string? YouTubeVideoId { get; set; }

    public string? MeetingUrl { get; set; }

    public DateTime? MeetingDateTime { get; set; }
}

public class LessonGoalDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;
}

public class LessonResourceDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? FileType { get; set; }
}
