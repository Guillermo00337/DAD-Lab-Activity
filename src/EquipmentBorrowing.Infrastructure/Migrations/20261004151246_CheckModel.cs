using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_MaxActiveBorrowings",
                table: "Students",
                sql: "\"MaxActiveBorrowings\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_IsAvailable",
                table: "Equipment",
                column: "IsAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_MaxActiveBorrowings",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_IsAvailable",
                table: "Equipment");
        }
    }
}
