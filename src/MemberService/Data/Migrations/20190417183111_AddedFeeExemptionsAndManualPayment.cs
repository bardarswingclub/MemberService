namespace MemberService.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddedFeeExemptionsAndManualPayment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "StripeChargeId",
            table: "Payments",
            nullable: true,
            oldClrType: typeof(string));

        migrationBuilder.AddColumn<string>(
            name: "ManualPayment",
            table: "Payments",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "ExemptFromClassesFee",
            table: "AspNetUsers",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "ExemptFromTrainingFee",
            table: "AspNetUsers",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ManualPayment",
            table: "Payments");

        migrationBuilder.DropColumn(
            name: "ExemptFromClassesFee",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "ExemptFromTrainingFee",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<string>(
            name: "StripeChargeId",
            table: "Payments",
            nullable: false,
            oldClrType: typeof(string),
            oldNullable: true);
    }
}
