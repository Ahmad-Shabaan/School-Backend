namespace Domain.Entities;

public class Quiz
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public decimal PassingScorePercent { get; set; } = 80m;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
