using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations;

[DbContext(typeof(EquipmentBorrowingDbContext))]
partial class EquipmentBorrowingDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("EquipmentBorrowing.Domain.Student", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<string>("StudentNumber").IsRequired().HasMaxLength(40).HasColumnType("character varying(40)");
            b.Property<string>("FullName").IsRequired().HasMaxLength(150).HasColumnType("character varying(150)");
            b.Property<string>("ContactInformation").HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<bool>("IsAllowedToBorrow").HasColumnType("boolean");
            b.Property<int>("MaxActiveBorrowings").ValueGeneratedOnAdd().HasDefaultValue(3).HasColumnType("integer");
            b.HasKey("Id");
            b.ToTable("Students", t =>
            {
                t.HasCheckConstraint("CK_Students_StudentNumber_NotBlank", "length(btrim(\"StudentNumber\")) > 0");
                t.HasCheckConstraint("CK_Students_FullName_NotBlank", "length(btrim(\"FullName\")) > 0");
                t.HasCheckConstraint("CK_Students_MaxActiveBorrowings", "\"MaxActiveBorrowings\" BETWEEN 0 AND 3");
                t.HasCheckConstraint("CK_Students_Contact_NotBlank", "\"ContactInformation\" IS NULL OR length(btrim(\"ContactInformation\")) > 0");
            });
        });

        modelBuilder.Entity("EquipmentBorrowing.Domain.Equipment", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<string>("AssetTag").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("Name").IsRequired().HasMaxLength(150).HasColumnType("character varying(150)");
            b.Property<string>("Description").HasMaxLength(300).HasColumnType("character varying(300)");
            b.Property<bool>("IsAvailable").ValueGeneratedOnAdd().HasDefaultValue(true).HasColumnType("boolean");
            b.HasKey("Id");
            b.HasIndex("IsAvailable");
            b.ToTable("Equipment", t =>
            {
                t.HasCheckConstraint("CK_Equipment_AssetTag_NotBlank", "length(btrim(\"AssetTag\")) > 0");
                t.HasCheckConstraint("CK_Equipment_Name_NotBlank", "length(btrim(\"Name\")) > 0");
                t.HasCheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR length(btrim(\"Description\")) > 0");
            });
        });

        modelBuilder.Entity("EquipmentBorrowing.Domain.Borrowing", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<int>("StudentId").HasColumnType("integer");
            b.Property<int>("EquipmentId").HasColumnType("integer");
            b.Property<DateOnly>("DateBorrowed").HasColumnType("date");
            b.Property<DateOnly>("ExpectedReturnDate").HasColumnType("date");
            b.Property<DateOnly?>("DateReturned").HasColumnType("date");
            b.Property<int>("Status").HasColumnType("integer");
            b.HasKey("Id");
            b.HasIndex("EquipmentId").IsUnique().HasFilter("\"Status\" = 0").HasDatabaseName("UX_Borrowings_OneActivePerEquipment");
            b.HasIndex("EquipmentId", "DateBorrowed");
            b.HasIndex("Status", "ExpectedReturnDate");
            b.HasIndex("StudentId", "Status", "DateBorrowed");
            b.ToTable("Borrowings", t =>
            {
                t.HasCheckConstraint("CK_Borrowings_ExpectedAfterCheckout", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
                t.HasCheckConstraint("CK_Borrowings_ReturnDate", "\"DateReturned\" IS NULL OR \"DateReturned\" >= \"DateBorrowed\"");
                t.HasCheckConstraint("CK_Borrowings_StatusReturnConsistency", "(\"Status\" = 0 AND \"DateReturned\" IS NULL) OR (\"Status\" = 1 AND \"DateReturned\" IS NOT NULL)");
            });
        });

        modelBuilder.Entity("EquipmentBorrowing.Domain.Borrowing", b =>
        {
            b.HasOne("EquipmentBorrowing.Domain.Equipment", null)
                .WithMany()
                .HasForeignKey("EquipmentId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("EquipmentBorrowing.Domain.Student", null)
                .WithMany()
                .HasForeignKey("StudentId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
#pragma warning restore 612, 618
    }
}
