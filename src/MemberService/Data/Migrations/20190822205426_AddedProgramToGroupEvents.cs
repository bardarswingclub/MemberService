using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddedProgramToGroupEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PartnerEmail",
                table: "EventSignup",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_NormalizedEmail",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    SignupOpensAt = table.Column<DateTime>(nullable: true),
                    SignupClosesAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    Archived = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programs_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignup_PartnerEmail",
                table: "EventSignup",
                column: "PartnerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProgramId",
                table: "Events",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_CreatedBy",
                table: "Programs",
                column: "CreatedBy");

            migrationBuilder.InsertData(
                "Programs",
                new string[] { "Id", "Title", "Archived", "Type", "CreatedAt" },
                new object[] { 0, "Dummy", false, ProgramType.Unknown.ToString(), DateTime.UtcNow });

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Programs_ProgramId",
                table: "Events",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Programs_ProgramId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_EventSignup_PartnerEmail",
                table: "EventSignup");

            migrationBuilder.DropIndex(
                name: "IX_Events_ProgramId",
                table: "Events");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_NormalizedEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "PartnerEmail",
                table: "EventSignup",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256);
        }
    }
}
