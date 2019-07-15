using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddedEventAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSignupAuditEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EventSignupId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    OccuredAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSignupAuditEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSignupAuditEntry_EventSignup_EventSignupId",
                        column: x => x.EventSignupId,
                        principalTable: "EventSignup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSignupAuditEntry_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSignupAuditEntry_EventSignupId",
                table: "EventSignupAuditEntry",
                column: "EventSignupId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignupAuditEntry_UserId",
                table: "EventSignupAuditEntry",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSignupAuditEntry");
        }
    }
}
