namespace Domain.Entities;

public class LessonGoal
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public string Text { get; set; } = string.Empty;
}
