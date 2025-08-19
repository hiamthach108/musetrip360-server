using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class MuseumSubscriptionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MuseumId",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MuseumId",
                table: "Subscriptions",
                column: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Museums_MuseumId",
                table: "Subscriptions",
                column: "MuseumId",
                principalTable: "Museums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Museums_MuseumId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_MuseumId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MuseumId",
                table: "Subscriptions");
        }
    }
}
