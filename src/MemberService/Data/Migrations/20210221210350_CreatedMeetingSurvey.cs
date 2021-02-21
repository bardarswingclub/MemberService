using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class CreatedMeetingSurvey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnswerableFrom",
                table: "Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnswerableUntil",
                table: "Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SurveyId",
                table: "AnnualMeetings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnnualMeetings_SurveyId",
                table: "AnnualMeetings",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualMeetings_Surveys_SurveyId",
                table: "AnnualMeetings",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnualMeetings_Surveys_SurveyId",
                table: "AnnualMeetings");

            migrationBuilder.DropIndex(
                name: "IX_AnnualMeetings_SurveyId",
                table: "AnnualMeetings");

            migrationBuilder.DropColumn(
                name: "AnswerableFrom",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AnswerableUntil",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "AnnualMeetings");
        }
    }
}
