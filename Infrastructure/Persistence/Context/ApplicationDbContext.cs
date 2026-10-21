using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        AppRoles,
        Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    public DbSet<Course> Courses
        => Set<Course>();

    public DbSet<UserCourse> UserCourses
        => Set<UserCourse>();

    public DbSet<Lesson> Lessons
        => Set<Lesson>();

    public DbSet<LessonGoal> LessonGoals
        => Set<LessonGoal>();

    public DbSet<LessonResource> LessonResources
        => Set<LessonResource>();

    public DbSet<LessonProgress> LessonProgresses
        => Set<LessonProgress>();

    public DbSet<LessonWatchSegment> LessonWatchSegments
        => Set<LessonWatchSegment>();

    public DbSet<Quiz> Quizzes
        => Set<Quiz>();

    public DbSet<Question> Questions
        => Set<Question>();

    public DbSet<AnswerOption> AnswerOptions
        => Set<AnswerOption>();

    public DbSet<QuizAttempt> QuizAttempts
        => Set<QuizAttempt>();

    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers
        => Set<QuizAttemptAnswer>();

    public DbSet<LessonComment> LessonComments
        => Set<LessonComment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        builder.Entity<ApplicationUser>()
            .ToTable("Users");

        builder.Entity<AppRoles>()
            .ToTable("Roles");

        builder.Entity<IdentityUserRole<Guid>>()
            .ToTable("UserRoles");

        builder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("UserClaims");

        builder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("RoleClaims");

        builder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("UserLogins");

        builder.Entity<IdentityUserToken<Guid>>()
            .ToTable("UserTokens");
    }
}
