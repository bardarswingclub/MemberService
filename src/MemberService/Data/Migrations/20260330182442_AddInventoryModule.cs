using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunicationRecipients_AspNetUsers_RecipientUserId",
                table: "EventCommunicationRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications");

            migrationBuilder.CreateTable(
                name: "InventoryBorrows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBorrows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBorrows_AspNetUsers_BorrowedByUserId",
                        column: x => x.BorrowedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SomeConsentRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SomeConsentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SomeConsentRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubKategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Beskrivelse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Merke = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modell = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Detaljer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LengdeM = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Diameter = table.Column<int>(type: "int", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InInventory = table.Column<bool>(type: "bit", nullable: false),
                    Lokasjon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CurrentBorrowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAssets_InventoryBorrows_CurrentBorrowId",
                        column: x => x.CurrentBorrowId,
                        principalTable: "InventoryBorrows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryBorrowItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BorrowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBorrowItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBorrowItems_InventoryAssets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "InventoryAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryBorrowItems_InventoryBorrows_BorrowId",
                        column: x => x.BorrowId,
                        principalTable: "InventoryBorrows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAssets_CurrentBorrowId",
                table: "InventoryAssets",
                column: "CurrentBorrowId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAssets_Tag",
                table: "InventoryAssets",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBorrowItems_AssetId",
                table: "InventoryBorrowItems",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBorrowItems_BorrowId_AssetId",
                table: "InventoryBorrowItems",
                columns: new[] { "BorrowId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBorrows_BorrowedByUserId",
                table: "InventoryBorrows",
                column: "BorrowedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SomeConsentRecords_UserId_ChangedAtUtc",
                table: "SomeConsentRecords",
                columns: new[] { "UserId", "ChangedAtUtc" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunicationRecipients_AspNetUsers_RecipientUserId",
                table: "EventCommunicationRecipients",
                column: "RecipientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications",
                column: "SentByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunicationRecipients_AspNetUsers_RecipientUserId",
                table: "EventCommunicationRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications");

            migrationBuilder.DropTable(
                name: "InventoryBorrowItems");

            migrationBuilder.DropTable(
                name: "SomeConsentRecords");

            migrationBuilder.DropTable(
                name: "InventoryAssets");

            migrationBuilder.DropTable(
                name: "InventoryBorrows");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunicationRecipients_AspNetUsers_RecipientUserId",
                table: "EventCommunicationRecipients",
                column: "RecipientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCommunications_AspNetUsers_SentByUserId",
                table: "EventCommunications",
                column: "SentByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
