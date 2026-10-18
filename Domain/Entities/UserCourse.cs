namespace Domain.Entities;

public class UserCourse
{
    public Guid ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    public int CourseId { get; set; }

    public Course Course { get; set; } = default!;
}