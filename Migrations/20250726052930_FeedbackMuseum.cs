using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackMuseum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Museums_MuseumId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_MuseumId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "MuseumId",
                table: "Feedbacks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MuseumId",
                table: "Feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_MuseumId",
                table: "Feedbacks",
                column: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Museums_MuseumId",
                table: "Feedbacks",
                column: "MuseumId",
                principalTable: "Museums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
