namespace Application.Features.Quizzes.DTOs;

public class QuizDto
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal PassingScorePercent { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public List<AnswerOptionDto> Options { get; set; } = new();
}

public class AnswerOptionDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;
}
