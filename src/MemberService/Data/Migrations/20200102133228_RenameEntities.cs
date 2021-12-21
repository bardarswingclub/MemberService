namespace MemberService.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class RenameEntities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_EventSignup_Events_EventId",
            table: "EventSignup");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignup_Payments_PaymentId",
            table: "EventSignup");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignup_AspNetUsers_UserId",
            table: "EventSignup");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignupAuditEntry_EventSignup_EventSignupId",
            table: "EventSignupAuditEntry");

        migrationBuilder.DropForeignKey(
            name: "FK_Presence_EventSignup_SignupId",
            table: "Presence");

        migrationBuilder.DropForeignKey(
            name: "FK_QuestionAnswers_EventSignup_SignupId",
            table: "QuestionAnswers");

        migrationBuilder.DropPrimaryKey(
            name: "PK_EventSignup",
            table: "EventSignup");

        migrationBuilder.RenameTable(
            name: "EventSignup",
            newName: "EventSignups");

        migrationBuilder.RenameColumn(
            name: "id",
            table: "QuestionOptions",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "id",
            table: "QuestionAnswers",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "SignupId",
            table: "QuestionAnswers",
            newName: "EventSignupId");

        migrationBuilder.RenameIndex(
            name: "IX_QuestionAnswers_SignupId",
            table: "QuestionAnswers",
            newName: "IX_QuestionAnswers_EventSignupId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignup_UserId",
            table: "EventSignups",
            newName: "IX_EventSignups_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignup_PaymentId",
            table: "EventSignups",
            newName: "IX_EventSignups_PaymentId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignup_EventId",
            table: "EventSignups",
            newName: "IX_EventSignups_EventId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_EventSignups",
            table: "EventSignups",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignupAuditEntry_EventSignups_EventSignupId",
            table: "EventSignupAuditEntry",
            column: "EventSignupId",
            principalTable: "EventSignups",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignups_Events_EventId",
            table: "EventSignups",
            column: "EventId",
            principalTable: "Events",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignups_Payments_PaymentId",
            table: "EventSignups",
            column: "PaymentId",
            principalTable: "Payments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignups_AspNetUsers_UserId",
            table: "EventSignups",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Presence_EventSignups_SignupId",
            table: "Presence",
            column: "SignupId",
            principalTable: "EventSignups",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_QuestionAnswers_EventSignups_EventSignupId",
            table: "QuestionAnswers",
            column: "EventSignupId",
            principalTable: "EventSignups",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_EventSignupAuditEntry_EventSignups_EventSignupId",
            table: "EventSignupAuditEntry");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignups_Events_EventId",
            table: "EventSignups");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignups_Payments_PaymentId",
            table: "EventSignups");

        migrationBuilder.DropForeignKey(
            name: "FK_EventSignups_AspNetUsers_UserId",
            table: "EventSignups");

        migrationBuilder.DropForeignKey(
            name: "FK_Presence_EventSignups_SignupId",
            table: "Presence");

        migrationBuilder.DropForeignKey(
            name: "FK_QuestionAnswers_EventSignups_EventSignupId",
            table: "QuestionAnswers");

        migrationBuilder.DropPrimaryKey(
            name: "PK_EventSignups",
            table: "EventSignups");

        migrationBuilder.RenameTable(
            name: "EventSignups",
            newName: "EventSignup");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "QuestionOptions",
            newName: "id");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "QuestionAnswers",
            newName: "id");

        migrationBuilder.RenameColumn(
            name: "EventSignupId",
            table: "QuestionAnswers",
            newName: "SignupId");

        migrationBuilder.RenameIndex(
            name: "IX_QuestionAnswers_EventSignupId",
            table: "QuestionAnswers",
            newName: "IX_QuestionAnswers_SignupId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignups_UserId",
            table: "EventSignup",
            newName: "IX_EventSignup_UserId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignups_PaymentId",
            table: "EventSignup",
            newName: "IX_EventSignup_PaymentId");

        migrationBuilder.RenameIndex(
            name: "IX_EventSignups_EventId",
            table: "EventSignup",
            newName: "IX_EventSignup_EventId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_EventSignup",
            table: "EventSignup",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignup_Events_EventId",
            table: "EventSignup",
            column: "EventId",
            principalTable: "Events",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignup_Payments_PaymentId",
            table: "EventSignup",
            column: "PaymentId",
            principalTable: "Payments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignup_AspNetUsers_UserId",
            table: "EventSignup",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_EventSignupAuditEntry_EventSignup_EventSignupId",
            table: "EventSignupAuditEntry",
            column: "EventSignupId",
            principalTable: "EventSignup",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Presence_EventSignup_SignupId",
            table: "Presence",
            column: "SignupId",
            principalTable: "EventSignup",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_QuestionAnswers_EventSignup_SignupId",
            table: "QuestionAnswers",
            column: "SignupId",
            principalTable: "EventSignup",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
