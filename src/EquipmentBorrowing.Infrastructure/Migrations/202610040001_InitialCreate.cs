using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EquipmentBorrowing.Infrastructure.Persistence;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations;

[DbContext(typeof(EquipmentBorrowingDbContext))]
[Migration("202610040001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable(
            name: "Equipment",
            columns: t => new
            {
                Id = t.Column<int>(
                    type: "INTEGER",
                    nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),

                AssetTag = t.Column<string>(
                    type: "TEXT",
                    maxLength: 50,
                    nullable: false),

                Name = t.Column<string>(
                    type: "TEXT",
                    maxLength: 150,
                    nullable: false),

                IsAvailable = t.Column<bool>(
                    type: "INTEGER",
                    nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Equipment", x => x.Id);
            });

        m.CreateTable(
            name: "Students",
            columns: t => new
            {
                Id = t.Column<int>(
                    type: "INTEGER",
                    nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),

                StudentNumber = t.Column<string>(
                    type: "TEXT",
                    maxLength: 30,
                    nullable: false),

                FullName = t.Column<string>(
                    type: "TEXT",
                    maxLength: 150,
                    nullable: false),

                IsAllowedToBorrow = t.Column<bool>(
                    type: "INTEGER",
                    nullable: false),

                MaxActiveBorrowings = t.Column<int>(
                    type: "INTEGER",
                    nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Students", x => x.Id);
            });

        m.CreateTable(
            name: "Borrowings",
            columns: t => new
            {
                Id = t.Column<int>(
                    type: "INTEGER",
                    nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),

                StudentId = t.Column<int>(
                    type: "INTEGER",
                    nullable: false),

                EquipmentId = t.Column<int>(
                    type: "INTEGER",
                    nullable: false),

                DateBorrowed = t.Column<string>(
                    type: "TEXT",
                    nullable: false),

                ExpectedReturnDate = t.Column<string>(
                    type: "TEXT",
                    nullable: false),

                DateReturned = t.Column<string>(
                    type: "TEXT",
                    nullable: true),

                Status = t.Column<int>(
                    type: "INTEGER",
                    nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Borrowings", x => x.Id);

                t.ForeignKey(
                    "FK_Borrowings_Equipment_EquipmentId",
                    x => x.EquipmentId,
                    "Equipment",
                    "Id",
                    onDelete: ReferentialAction.Restrict);

                t.ForeignKey(
                    "FK_Borrowings_Students_StudentId",
                    x => x.StudentId,
                    "Students",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        m.CreateIndex(
            "IX_Equipment_AssetTag",
            "Equipment",
            "AssetTag",
            unique: true);

        m.CreateIndex(
            "IX_Students_StudentNumber",
            "Students",
            "StudentNumber",
            unique: true);

        m.CreateIndex(
            "IX_Borrowings_EquipmentId_Status",
            "Borrowings",
            new[] { "EquipmentId", "Status" });

        m.CreateIndex(
            "IX_Borrowings_StudentId_Status",
            "Borrowings",
            new[] { "StudentId", "Status" });
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DropTable("Borrowings");
        m.DropTable("Equipment");
        m.DropTable("Students");
    }
}