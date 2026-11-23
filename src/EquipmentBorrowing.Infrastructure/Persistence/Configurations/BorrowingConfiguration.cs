using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", table =>
        {
            table.HasCheckConstraint("CK_Borrowings_ExpectedAfterCheckout", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
            table.HasCheckConstraint("CK_Borrowings_ReturnDate", "\"DateReturned\" IS NULL OR \"DateReturned\" >= \"DateBorrowed\"");
            table.HasCheckConstraint("CK_Borrowings_StatusReturnConsistency", "(\"Status\" = 0 AND \"DateReturned\" IS NULL) OR (\"Status\" = 1 AND \"DateReturned\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.EquipmentId).IsRequired();
        builder.Property(x => x.DateBorrowed).HasColumnType("date").IsRequired();
        builder.Property(x => x.ExpectedReturnDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DateReturned).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasOne<Student>().WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Equipment>().WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.StudentId, x.Status, x.DateBorrowed });
        builder.HasIndex(x => new { x.EquipmentId, x.DateBorrowed });
        builder.HasIndex(x => new { x.Status, x.ExpectedReturnDate });
        builder.HasIndex(x => x.EquipmentId).IsUnique().HasFilter("\"Status\" = 0").HasDatabaseName("UX_Borrowings_OneActivePerEquipment");
    }
}
