namespace Domain.Entities;

public class Question
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = default!;

    public string Text { get; set; } = string.Empty;

    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}
