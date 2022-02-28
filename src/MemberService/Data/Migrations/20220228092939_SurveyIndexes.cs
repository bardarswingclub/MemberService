using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    public partial class SurveyIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_SurveyId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Events_SurveyId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_SurveyId",
                table: "Semesters",
                column: "SurveyId",
                unique: true,
                filter: "[SurveyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SurveyId",
                table: "Events",
                column: "SurveyId",
                unique: true,
                filter: "[SurveyId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_SurveyId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Events_SurveyId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_SurveyId",
                table: "Semesters",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SurveyId",
                table: "Events",
                column: "SurveyId");
        }
    }
}
