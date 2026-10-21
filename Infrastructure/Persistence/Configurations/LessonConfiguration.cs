using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.YouTubeVideoId)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(32);

        builder.Property(x => x.MeetingUrl)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.CourseId, x.Order })
            .IsUnique();

        builder.HasOne(x => x.Course)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
