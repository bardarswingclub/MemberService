using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddedAutoAcceptSignups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoAcceptedSignups",
                table: "EventSignupOptions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoAcceptedSignups",
                table: "EventSignupOptions");
        }
    }
}
