using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResource>
{
    public void Configure(EntityTypeBuilder<LessonResource> builder)
    {
        builder.ToTable("LessonResources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FileUrl)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.FileType)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(100);

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Resources)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
