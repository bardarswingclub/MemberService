using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPresenceAbsenceReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbsenceNote",
                table: "Presence",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AbsenceReason",
                table: "Presence",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsenceNote",
                table: "Presence");

            migrationBuilder.DropColumn(
                name: "AbsenceReason",
                table: "Presence");
        }
    }
}
