using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LessonCommentConfiguration : IEntityTypeConfiguration<LessonComment>
{
    public void Configure(EntityTypeBuilder<LessonComment> builder)
    {
        builder.ToTable("LessonComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Content)
            .HasColumnType("NVARCHAR")
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(x => new { x.LessonId, x.CreatedAt });

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
