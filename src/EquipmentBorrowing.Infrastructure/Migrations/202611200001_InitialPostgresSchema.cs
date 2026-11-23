using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations;

[DbContext(typeof(EquipmentBorrowingDbContext))]
[Migration("202611200001_InitialPostgresSchema")]
public partial class InitialPostgresSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                ContactInformation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                IsAllowedToBorrow = table.Column<bool>(type: "boolean", nullable: false),
                MaxActiveBorrowings = table.Column<int>(type: "integer", nullable: false, defaultValue: 3)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
                table.CheckConstraint("CK_Students_StudentNumber_NotBlank", "length(btrim(\"StudentNumber\")) > 0");
                table.CheckConstraint("CK_Students_FullName_NotBlank", "length(btrim(\"FullName\")) > 0");
                table.CheckConstraint("CK_Students_MaxActiveBorrowings", "\"MaxActiveBorrowings\" BETWEEN 0 AND 3");
                table.CheckConstraint("CK_Students_Contact_NotBlank", "\"ContactInformation\" IS NULL OR length(btrim(\"ContactInformation\")) > 0");
            });

        migrationBuilder.CreateTable(
            name: "Equipment",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AssetTag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Equipment", x => x.Id);
                table.CheckConstraint("CK_Equipment_AssetTag_NotBlank", "length(btrim(\"AssetTag\")) > 0");
                table.CheckConstraint("CK_Equipment_Name_NotBlank", "length(btrim(\"Name\")) > 0");
                table.CheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR length(btrim(\"Description\")) > 0");
            });

        migrationBuilder.CreateTable(
            name: "Borrowings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentId = table.Column<int>(type: "integer", nullable: false),
                EquipmentId = table.Column<int>(type: "integer", nullable: false),
                DateBorrowed = table.Column<DateOnly>(type: "date", nullable: false),
                ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                DateReturned = table.Column<DateOnly>(type: "date", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Borrowings", x => x.Id);
                table.ForeignKey("FK_Borrowings_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Borrowings_Equipment_EquipmentId", x => x.EquipmentId, "Equipment", "Id", onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_Borrowings_ExpectedAfterCheckout", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
                table.CheckConstraint("CK_Borrowings_ReturnDate", "\"DateReturned\" IS NULL OR \"DateReturned\" >= \"DateBorrowed\"");
                table.CheckConstraint("CK_Borrowings_StatusReturnConsistency", "(\"Status\" = 0 AND \"DateReturned\" IS NULL) OR (\"Status\" = 1 AND \"DateReturned\" IS NOT NULL)");
            });

        migrationBuilder.CreateIndex("IX_Equipment_IsAvailable", "Equipment", "IsAvailable");
        migrationBuilder.CreateIndex("IX_Borrowings_StudentId_Status_DateBorrowed", "Borrowings", new[] { "StudentId", "Status", "DateBorrowed" });
        migrationBuilder.CreateIndex("IX_Borrowings_EquipmentId_DateBorrowed", "Borrowings", new[] { "EquipmentId", "DateBorrowed" });
        migrationBuilder.CreateIndex("IX_Borrowings_Status_ExpectedReturnDate", "Borrowings", new[] { "Status", "ExpectedReturnDate" });

        migrationBuilder.Sql("CREATE UNIQUE INDEX \"UX_Students_StudentNumber_Normalized\" ON \"Students\" (upper(btrim(\"StudentNumber\")))");
        migrationBuilder.Sql("CREATE UNIQUE INDEX \"UX_Equipment_AssetTag_Normalized\" ON \"Equipment\" (upper(btrim(\"AssetTag\")))");
        migrationBuilder.Sql("CREATE UNIQUE INDEX \"UX_Borrowings_OneActivePerEquipment\" ON \"Borrowings\" (\"EquipmentId\") WHERE \"Status\" = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Borrowings");
        migrationBuilder.DropTable("Equipment");
        migrationBuilder.DropTable("Students");
    }
}
