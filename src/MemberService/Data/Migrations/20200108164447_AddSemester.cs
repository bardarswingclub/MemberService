namespace MemberService.Data.Migrations;



using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddSemester : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "SemesterId",
            table: "Events",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Semesters",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(nullable: true),
                SignupOpensAt = table.Column<DateTime>(nullable: false),
                SurveyId = table.Column<Guid>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Semesters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Semesters_Surveys_SurveyId",
                    column: x => x.SurveyId,
                    principalTable: "Surveys",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Events_SemesterId",
            table: "Events",
            column: "SemesterId");

        migrationBuilder.CreateIndex(
            name: "IX_Semesters_SurveyId",
            table: "Semesters",
            column: "SurveyId");

        migrationBuilder.AddForeignKey(
            name: "FK_Events_Semesters_SemesterId",
            table: "Events",
            column: "SemesterId",
            principalTable: "Semesters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Events_Semesters_SemesterId",
            table: "Events");

        migrationBuilder.DropTable(
            name: "Semesters");

        migrationBuilder.DropIndex(
            name: "IX_Events_SemesterId",
            table: "Events");

        migrationBuilder.DropColumn(
            name: "SemesterId",
            table: "Events");
    }
}
