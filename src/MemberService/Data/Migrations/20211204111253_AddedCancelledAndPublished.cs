namespace MemberService.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddedCancelledAndPublished : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Cancelled",
            table: "Events",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "Published",
            table: "Events",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Cancelled",
            table: "Events");

        migrationBuilder.DropColumn(
            name: "Published",
            table: "Events");
    }
}
