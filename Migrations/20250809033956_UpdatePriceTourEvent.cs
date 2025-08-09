using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriceTourEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "TourOnlines",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "Events",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DataEntityType",
                table: "Articles",
                type: "text",
                nullable: false,
                defaultValue: "Museum",
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "TourOnlines");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Articles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Draft");

            migrationBuilder.AlterColumn<int>(
                name: "DataEntityType",
                table: "Articles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Museum");
        }
    }
}
