namespace Application.Features.Quizzes.DTOs;

public class QuizManagementDto
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal PassingScorePercent { get; set; }

    public int AttemptCount { get; set; }

    public List<QuizManagementQuestionDto> Questions { get; set; } = new();
}

public class QuizManagementQuestionDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public List<QuizManagementOptionDto> Options { get; set; } = new();
}

public class QuizManagementOptionDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

public interface IQuizPayload
{
    string Title { get; }

    decimal PassingScorePercent { get; }

    List<QuizQuestionInput> Questions { get; }
}

public class QuizQuestionInput
{
    public string Text { get; set; } = string.Empty;

    public List<QuizOptionInput> Options { get; set; } = new();
}

public class QuizOptionInput
{
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
