namespace MemberService.Data.Migrations;



using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddPresenceTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "LessonCount",
            table: "Events",
            nullable: false,
            defaultValue: 0);
        migrationBuilder.CreateTable(
            name: "Presence",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                SignupId = table.Column<Guid>(nullable: false),
                Lesson = table.Column<int>(nullable: false),
                Present = table.Column<bool>(nullable: false),
                RegisteredAt = table.Column<DateTime>(nullable: false),
                RegisteredById = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Presence", x => x.Id);
                table.ForeignKey(
                    name: "FK_Presence_AspNetUsers_RegisteredById",
                    column: x => x.RegisteredById,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Presence_EventSignup_SignupId",
                    column: x => x.SignupId,
                    principalTable: "EventSignup",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Presence_RegisteredById",
            table: "Presence",
            column: "RegisteredById");

        migrationBuilder.CreateIndex(
            name: "IX_Presence_SignupId",
            table: "Presence",
            column: "SignupId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Presence");

        migrationBuilder.DropColumn(
            name: "LessonCount",
            table: "Events");
    }
}
