using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackAndTourStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuseumBalances");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TourOnlines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "Feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Feedbacks",
                type: "text",
                nullable: false,
                defaultValue: "Museum");

            migrationBuilder.CreateTable(
                name: "MuseumWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MuseumId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableBalance = table.Column<float>(type: "real", nullable: false),
                    PendingBalance = table.Column<float>(type: "real", nullable: false),
                    TotalBalance = table.Column<float>(type: "real", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumWallets_Museums_MuseumId",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuseumWallets_MuseumId",
                table: "MuseumWallets",
                column: "MuseumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuseumWallets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TourOnlines");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Feedbacks");

            migrationBuilder.CreateTable(
                name: "MuseumBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MuseumId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableBalance = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PendingBalance = table.Column<float>(type: "real", nullable: false),
                    TotalBalance = table.Column<float>(type: "real", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumBalances_Museums_MuseumId",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuseumBalances_MuseumId",
                table: "MuseumBalances",
                column: "MuseumId");
        }
    }
}
