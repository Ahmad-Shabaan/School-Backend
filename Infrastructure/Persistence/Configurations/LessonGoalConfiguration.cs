using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LessonGoalConfiguration : IEntityTypeConfiguration<LessonGoal>
{
    public void Configure(EntityTypeBuilder<LessonGoal> builder)
    {
        builder.ToTable("LessonGoals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Text)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
