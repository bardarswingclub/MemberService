using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastObservedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications");

            migrationBuilder.AlterColumn<decimal>(
                name: "LengdeM",
                table: "InventoryAssets",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastObservedAt",
                table: "InventoryAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications",
                column: "SentByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications");

            migrationBuilder.DropColumn(
                name: "LastObservedAt",
                table: "InventoryAssets");

            migrationBuilder.AlterColumn<decimal>(
                name: "LengdeM",
                table: "InventoryAssets",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications",
                column: "SentByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
