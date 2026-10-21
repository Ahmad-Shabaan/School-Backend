namespace Domain.Entities;

public class QuizAttemptAnswer
{
    public int Id { get; set; }

    public int QuizAttemptId { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = default!;

    public int QuestionId { get; set; }

    public Question Question { get; set; } = default!;

    public int? SelectedOptionId { get; set; }

    public AnswerOption? SelectedOption { get; set; }

    public decimal PointsAwarded { get; set; }
}
