using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentAdminTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedByAdminId",
                table: "SomeConsentRecords",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SomeConsentRecords_ChangedByAdminId",
                table: "SomeConsentRecords",
                column: "ChangedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_SomeConsentRecords_AspNetUsers_ChangedByAdminId",
                table: "SomeConsentRecords",
                column: "ChangedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SomeConsentRecords_AspNetUsers_ChangedByAdminId",
                table: "SomeConsentRecords");

            migrationBuilder.DropIndex(
                name: "IX_SomeConsentRecords_ChangedByAdminId",
                table: "SomeConsentRecords");

            migrationBuilder.DropColumn(
                name: "ChangedByAdminId",
                table: "SomeConsentRecords");
        }
    }
}
