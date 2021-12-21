namespace MemberService.Data.Migrations;



using Microsoft.EntityFrameworkCore.Migrations;

public partial class RecordAnsweredAt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "AnsweredAt",
            table: "QuestionAnswers",
            type: "datetime2",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AnsweredAt",
            table: "QuestionAnswers");
    }
}
