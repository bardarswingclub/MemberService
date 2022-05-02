using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    public partial class TemporalHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SemesterRoles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "EventOrganizers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "EventOrganizersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                table: "SemesterRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "SemesterRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SemesterRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SemesterRoles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                table: "EventOrganizers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "EventOrganizers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterRoles_UpdatedBy",
                table: "SemesterRoles",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterRoles_AspNetUsers_UpdatedBy",
                table: "SemesterRoles",
                column: "UpdatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SemesterRoles_AspNetUsers_UpdatedBy",
                table: "SemesterRoles");

            migrationBuilder.DropIndex(
                name: "IX_SemesterRoles_UpdatedBy",
                table: "SemesterRoles");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "SemesterRoles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "SemesterRoles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SemesterRoles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null);

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SemesterRoles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null);

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "EventOrganizers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "EventOrganizersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "EventOrganizers")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "EventOrganizersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "SemesterRoles")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "SemesterRolesHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AlterTable(
                name: "EventOrganizers")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "EventOrganizersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", null)
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }
    }
}
