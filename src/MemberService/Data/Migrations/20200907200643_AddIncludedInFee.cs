using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddIncludedInFee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludedInClassesFee",
                table: "EventSignupOptions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludedInTrainingFee",
                table: "EventSignupOptions",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludedInClassesFee",
                table: "EventSignupOptions");

            migrationBuilder.DropColumn(
                name: "IncludedInTrainingFee",
                table: "EventSignupOptions");
        }
    }
}
