using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment", table =>
        {
            table.HasCheckConstraint("CK_Equipment_AssetTag_NotBlank", "length(btrim(\"AssetTag\")) > 0");
            table.HasCheckConstraint("CK_Equipment_Name_NotBlank", "length(btrim(\"Name\")) > 0");
            table.HasCheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR length(btrim(\"Description\")) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssetTag).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.Property(x => x.IsAvailable).HasDefaultValue(true).IsRequired();
        builder.HasIndex(x => x.IsAvailable);
    }
}
