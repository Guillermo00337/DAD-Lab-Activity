using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", table =>
        {
            table.HasCheckConstraint("CK_Students_StudentNumber_NotBlank", "length(btrim(\"StudentNumber\")) > 0");
            table.HasCheckConstraint("CK_Students_FullName_NotBlank", "length(btrim(\"FullName\")) > 0");
            table.HasCheckConstraint("CK_Students_MaxActiveBorrowings", "\"MaxActiveBorrowings\" BETWEEN 0 AND 3");
            table.HasCheckConstraint("CK_Students_Contact_NotBlank", "\"ContactInformation\" IS NULL OR length(btrim(\"ContactInformation\")) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ContactInformation).HasMaxLength(200);
        builder.Property(x => x.IsAllowedToBorrow).IsRequired();
        builder.Property(x => x.MaxActiveBorrowings).HasDefaultValue(3).IsRequired();
    }
}
