using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);

        builder.Property(student => student.StudentNumber)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(student => student.StudentNumber).IsUnique();

        builder.Property(student => student.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(student => student.IsAllowedToBorrow).IsRequired();
        builder.Property(student => student.MaxActiveBorrowings).IsRequired();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Students_MaxActiveBorrowings",
            "\"MaxActiveBorrowings\" >= 0"));
    }
}
