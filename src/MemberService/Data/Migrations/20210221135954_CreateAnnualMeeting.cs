namespace MemberService.Data.Migrations;



using Microsoft.EntityFrameworkCore.Migrations;

public partial class CreateAnnualMeeting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AnnualMeetings",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(nullable: false),
                MeetingInvitation = table.Column<string>(nullable: true),
                MeetingInfo = table.Column<string>(nullable: true),
                MeetingSummary = table.Column<string>(nullable: true),
                MeetingStartsAt = table.Column<DateTime>(nullable: false),
                MeetingEndsAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnnualMeetings", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AnnualMeetings");
    }
}
