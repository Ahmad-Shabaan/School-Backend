namespace Domain.Entities;

public class LessonWatchSegment
{
    public int Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public double StartSeconds { get; set; }

    public double EndSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
