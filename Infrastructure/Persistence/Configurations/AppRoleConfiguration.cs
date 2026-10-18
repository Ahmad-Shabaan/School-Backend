using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRoles>
{
    public void Configure(EntityTypeBuilder<AppRoles> builder)
    {
        builder.ToTable("Roles");

        builder.Property(x => x.Name)
            .HasMaxLength(100);

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
