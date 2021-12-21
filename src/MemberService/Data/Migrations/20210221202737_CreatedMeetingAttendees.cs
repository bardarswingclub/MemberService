namespace MemberService.Data.Migrations;



using Microsoft.EntityFrameworkCore.Migrations;

public partial class CreatedMeetingAttendees : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AnnualMeetingAttendee",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AnnualMeetingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastVisited = table.Column<DateTime>(type: "datetime2", nullable: false),
                Visits = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnnualMeetingAttendee", x => x.Id);
                table.ForeignKey(
                    name: "FK_AnnualMeetingAttendee_AnnualMeetings_AnnualMeetingId",
                    column: x => x.AnnualMeetingId,
                    principalTable: "AnnualMeetings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AnnualMeetingAttendee_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AnnualMeetingAttendee_AnnualMeetingId",
            table: "AnnualMeetingAttendee",
            column: "AnnualMeetingId");

        migrationBuilder.CreateIndex(
            name: "IX_AnnualMeetingAttendee_UserId",
            table: "AnnualMeetingAttendee",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AnnualMeetingAttendee");
    }
}
