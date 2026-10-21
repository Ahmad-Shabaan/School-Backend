namespace Domain.Entities;

public class QuizAttempt
{
    public int Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = default!;

    public int AttemptNumber { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}
