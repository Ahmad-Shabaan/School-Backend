namespace Domain.Entities;

public class LessonComment
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;

    public Guid AuthorId { get; set; }

    public ApplicationUser Author { get; set; } = default!;

    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public LessonComment? ParentComment { get; set; }

    public ICollection<LessonComment> Replies { get; set; } = new List<LessonComment>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
