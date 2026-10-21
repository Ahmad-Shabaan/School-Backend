namespace Domain.Entities;

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public string? YouTubeVideoId { get; set; }

    public string? MeetingUrl { get; set; }

    public DateTime? MeetingDateTime { get; set; }

    public ICollection<LessonGoal> Goals { get; set; } = new List<LessonGoal>();

    public ICollection<LessonResource> Resources { get; set; } = new List<LessonResource>();

    public Quiz? Quiz { get; set; }

    public ICollection<LessonComment> Comments { get; set; } = new List<LessonComment>();
}
