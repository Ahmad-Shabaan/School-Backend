namespace Application.Features.Comments.DTOs;

public class CommentDto
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public int? ParentCommentId { get; set; }

    public Guid AuthorId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<CommentDto> Replies { get; set; } = new();
}
