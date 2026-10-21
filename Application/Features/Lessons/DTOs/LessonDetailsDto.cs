namespace Application.Features.Lessons.DTOs;

public class LessonDetailsDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public string? YouTubeVideoId { get; set; }

    public string? MeetingUrl { get; set; }

    public DateTime? MeetingDateTime { get; set; }

    public List<LessonGoalDto> Goals { get; set; } = new();

    public List<LessonResourceDto> Resources { get; set; } = new();

    public int? QuizId { get; set; }

    public bool HasQuiz { get; set; }

    public LessonAccessDto Access { get; set; } = new();
}

public class LessonAccessDto
{
    public bool IsAccessible { get; set; }

    public bool IsEnrolled { get; set; }

    public bool IsVideoCompleted { get; set; }

    public bool IsQuizPassed { get; set; }

    public string? MissingRequirement { get; set; }
}

public class LessonStatusDto
{
    public int Id { get; set; }

    public int Order { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsUnlocked { get; set; }

    public string? MeetingUrl { get; set; }

    public DateTime? MeetingDateTime { get; set; }

    public decimal WatchedPercentage { get; set; }

    public bool IsVideoCompleted { get; set; }

    public bool IsQuizPassed { get; set; }

    public bool HasQuiz { get; set; }

    public bool IsCompleted => IsVideoCompleted && (!HasQuiz || IsQuizPassed);
}
