namespace MemberService.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddedHelpText : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AllowPartnerSignupHelp",
            table: "EventSignupOptions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RoleSignupHelp",
            table: "EventSignupOptions",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AllowPartnerSignupHelp",
            table: "EventSignupOptions");

        migrationBuilder.DropColumn(
            name: "RoleSignupHelp",
            table: "EventSignupOptions");
    }
}
