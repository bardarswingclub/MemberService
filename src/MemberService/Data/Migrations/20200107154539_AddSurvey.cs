using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddSurvey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSignups_AspNetUsers_UserId",
                table: "EventSignups");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionAnswers_EventSignups_EventSignupId",
                table: "QuestionAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Events_EventId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "Questions",
                newName: "SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_EventId",
                table: "Questions",
                newName: "IX_Questions_SurveyId");

            migrationBuilder.RenameColumn(
                name: "EventSignupId",
                table: "QuestionAnswers",
                newName: "ResponseId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionAnswers_EventSignupId",
                table: "QuestionAnswers",
                newName: "IX_QuestionAnswers_ResponseId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventSignups",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponseId",
                table: "EventSignups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SurveyId",
                table: "Events",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Response",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SurveyId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Response", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Response_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Response_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignups_ResponseId",
                table: "EventSignups",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SurveyId",
                table: "Events",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Response_SurveyId",
                table: "Response",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Response_UserId",
                table: "Response",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Surveys_SurveyId",
                table: "Events",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSignups_Response_ResponseId",
                table: "EventSignups",
                column: "ResponseId",
                principalTable: "Response",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSignups_AspNetUsers_UserId",
                table: "EventSignups",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionAnswers_Response_ResponseId",
                table: "QuestionAnswers",
                column: "ResponseId",
                principalTable: "Response",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Surveys_SurveyId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventSignups_Response_ResponseId",
                table: "EventSignups");

            migrationBuilder.DropForeignKey(
                name: "FK_EventSignups_AspNetUsers_UserId",
                table: "EventSignups");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionAnswers_Response_ResponseId",
                table: "QuestionAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Surveys_SurveyId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "Response");

            migrationBuilder.DropTable(
                name: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_EventSignups_ResponseId",
                table: "EventSignups");

            migrationBuilder.DropIndex(
                name: "IX_Events_SurveyId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ResponseId",
                table: "EventSignups");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "SurveyId",
                table: "Questions",
                newName: "EventId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_SurveyId",
                table: "Questions",
                newName: "IX_Questions_EventId");

            migrationBuilder.RenameColumn(
                name: "ResponseId",
                table: "QuestionAnswers",
                newName: "EventSignupId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionAnswers_ResponseId",
                table: "QuestionAnswers",
                newName: "IX_QuestionAnswers_EventSignupId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "EventSignups",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_EventSignups_AspNetUsers_UserId",
                table: "EventSignups",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionAnswers_EventSignups_EventSignupId",
                table: "QuestionAnswers",
                column: "EventSignupId",
                principalTable: "EventSignups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Events_EventId",
                table: "Questions",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
