using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class MessageCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_CreatedByUserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CreatedByUserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedBy",
                table: "Messages",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_CreatedBy",
                table: "Messages",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_CreatedBy",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_CreatedBy",
                table: "Messages");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedByUserId",
                table: "Messages",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_CreatedByUserId",
                table: "Messages",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
