namespace Application.Features.Quizzes.DTOs;

public class QuizAnswerInput
{
    public int QuestionId { get; set; }

    public int OptionId { get; set; }
}

public class QuizAttemptDto
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int AttemptNumber { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public List<QuizAttemptAnswerDto> Answers { get; set; } = new();
}

public class QuizAttemptAnswerDto
{
    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public decimal PointsAwarded { get; set; }
}
