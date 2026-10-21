using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PassingScorePercent)
            .HasColumnType("DECIMAL(5,2)");

        builder.HasIndex(x => x.LessonId)
            .IsUnique();

        builder.HasOne(x => x.Lesson)
            .WithOne(x => x.Quiz)
            .HasForeignKey<Quiz>(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
