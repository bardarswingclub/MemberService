using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class AddedEventSignups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventSignup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EventId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    SignedUpAt = table.Column<DateTime>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    PartnerEmail = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSignup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSignup_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSignup_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventSignupOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignupOpensAt = table.Column<DateTime>(nullable: true),
                    SignupClosesAt = table.Column<DateTime>(nullable: true),
                    RequiresMembershipFee = table.Column<bool>(nullable: false),
                    RequiresTrainingFee = table.Column<bool>(nullable: false),
                    RequiresClassesFee = table.Column<bool>(nullable: false),
                    PriceForMembers = table.Column<decimal>(nullable: false),
                    PriceForNonMembers = table.Column<decimal>(nullable: false),
                    AllowPartnerSignup = table.Column<bool>(nullable: false),
                    RoleSignup = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSignupOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSignupOptions_Events_Id",
                        column: x => x.Id,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatedBy",
                table: "Events",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignup_EventId",
                table: "EventSignup",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignup_UserId",
                table: "EventSignup",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignupOptions_SignupClosesAt",
                table: "EventSignupOptions",
                column: "SignupClosesAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventSignupOptions_SignupOpensAt",
                table: "EventSignupOptions",
                column: "SignupOpensAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSignup");

            migrationBuilder.DropTable(
                name: "EventSignupOptions");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
