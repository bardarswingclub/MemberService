namespace MemberService.Data.Migrations;

using MemberService.Data.ValueTypes;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddedEventTypeColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Type",
            table: "Events",
            nullable: false,
            defaultValue: EventType.Workshop.ToString());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Type",
            table: "Events");
    }
}
