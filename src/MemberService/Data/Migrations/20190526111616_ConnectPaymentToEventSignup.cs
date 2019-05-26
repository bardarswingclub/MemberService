using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class ConnectPaymentToEventSignup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "EventSignup",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventSignup_PaymentId",
                table: "EventSignup",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EventSignup_Payments_PaymentId",
                table: "EventSignup",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSignup_Payments_PaymentId",
                table: "EventSignup");

            migrationBuilder.DropIndex(
                name: "IX_EventSignup_PaymentId",
                table: "EventSignup");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "EventSignup");
        }
    }
}
