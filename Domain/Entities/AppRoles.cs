using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppRoles : IdentityRole<Guid>
{
    public const string Admin = "Admin";

    public const string PreAdmin = "PreAdmin";

    public const string Teacher = "Teacher";

    public const string Student = "Student";

    public string? Description { get; set; }
}
