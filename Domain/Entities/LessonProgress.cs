namespace Domain.Entities;

public class LessonProgress
{
    public int Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public double LastPositionSeconds { get; set; }

    public double DurationSeconds { get; set; }

    public decimal WatchedPercentage { get; set; }

    public bool IsWatched { get; set; }

    public DateTime? WatchedAt { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
