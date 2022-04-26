using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    public partial class AddVippsReservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VippsOrderId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VippsReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncludesMembership = table.Column<bool>(type: "bit", nullable: false),
                    IncludesTraining = table.Column<bool>(type: "bit", nullable: false),
                    IncludesClasses = table.Column<bool>(type: "bit", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VippsReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VippsReservations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VippsReservations_UserId",
                table: "VippsReservations",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VippsReservations");

            migrationBuilder.DropColumn(
                name: "VippsOrderId",
                table: "Payments");
        }
    }
}
