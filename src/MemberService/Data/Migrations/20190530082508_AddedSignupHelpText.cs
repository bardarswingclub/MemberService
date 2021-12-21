namespace MemberService.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddedSignupHelpText : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SignupHelp",
            table: "EventSignupOptions",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SignupHelp",
            table: "EventSignupOptions");
    }
}
