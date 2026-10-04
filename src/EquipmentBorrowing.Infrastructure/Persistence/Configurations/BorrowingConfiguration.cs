using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings");

        builder.HasKey(borrowing => borrowing.Id);

        builder.Property(borrowing => borrowing.StudentId)
            .IsRequired();

        builder.Property(borrowing => borrowing.EquipmentId)
            .IsRequired();

        builder.Property(borrowing => borrowing.DateBorrowed)
            .IsRequired();

        builder.Property(borrowing => borrowing.ExpectedReturnDate)
            .IsRequired();

        builder.Property(borrowing => borrowing.DateReturned)
            .IsRequired(false);

        builder.Property(borrowing => borrowing.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(borrowing => borrowing.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(borrowing => borrowing.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(borrowing => new
        {
            borrowing.StudentId,
            borrowing.Status
        });

        builder.HasIndex(borrowing => new
        {
            borrowing.EquipmentId,
            borrowing.Status
        });
    }
}