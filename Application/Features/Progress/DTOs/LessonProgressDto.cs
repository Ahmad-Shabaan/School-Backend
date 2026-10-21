namespace Application.Features.Progress.DTOs;

public class LessonProgressDto
{
    public int LessonId { get; set; }

    public double LastPositionSeconds { get; set; }

    public double DurationSeconds { get; set; }

    public decimal WatchedPercentage { get; set; }

    public bool IsVideoCompleted { get; set; }

    public DateTime UpdatedAt { get; set; }
}
