using Microsoft.EntityFrameworkCore.Migrations;

namespace MemberService.Data.Migrations
{
    public partial class PaymentShouldBeDecimalAndDateShouldBeUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayedAt",
                table: "Payments",
                newName: "PayedAtUtc");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayedAtUtc",
                table: "Payments",
                newName: "PayedAt");

            migrationBuilder.AlterColumn<long>(
                name: "Amount",
                table: "Payments",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
