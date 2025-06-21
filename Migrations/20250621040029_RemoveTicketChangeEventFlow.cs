using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuseTrip360.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicketChangeEventFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderTickets");

            migrationBuilder.DropTable(
                name: "TicketTicketAddon");

            migrationBuilder.DropTable(
                name: "TicketAddons");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketMasters");

            migrationBuilder.AddColumn<int>(
                name: "ZOrder",
                table: "MuseumPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Role = table.Column<string>(type: "text", nullable: false, defaultValue: "Attendee"),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderEvents",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvents", x => new { x.OrderId, x.EventId });
                    table.ForeignKey(
                        name: "FK_OrderEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderEvents_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderTours",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TourId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTours", x => new { x.OrderId, x.TourId });
                    table.ForeignKey(
                        name: "FK_OrderTours_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTours_TourOnlines_TourId",
                        column: x => x.TourId,
                        principalTable: "TourOnlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    MaxEvents = table.Column<int>(type: "integer", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepresentationMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ZOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepresentationMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepresentationMaterials_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepresentationMaterials_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourViewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TourId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourViewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourViewers_TourOnlines_TourId",
                        column: x => x.TourId,
                        principalTable: "TourOnlines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourViewers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_EventId",
                table: "EventParticipants",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_UserId",
                table: "EventParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvents_EventId",
                table: "OrderEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvents_OrderId",
                table: "OrderEvents",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTours_OrderId",
                table: "OrderTours",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTours_TourId",
                table: "OrderTours",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepresentationMaterials_CreatedBy",
                table: "RepresentationMaterials",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RepresentationMaterials_EventId",
                table: "RepresentationMaterials",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OrderId",
                table: "Subscriptions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TourViewers_TourId",
                table: "TourViewers",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourViewers_UserId",
                table: "TourViewers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "OrderEvents");

            migrationBuilder.DropTable(
                name: "OrderTours");

            migrationBuilder.DropTable(
                name: "RepresentationMaterials");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "TourViewers");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropColumn(
                name: "ZOrder",
                table: "MuseumPolicies");

            migrationBuilder.CreateTable(
                name: "TicketAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuseumId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAddons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAddons_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketAddons_Museums_MuseumId",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MuseumId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DiscountPercentage = table.Column<float>(type: "real", nullable: false),
                    GroupSize = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMasters_Museums_MuseumId",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketMasters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderTickets",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupSize = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTickets", x => new { x.OrderId, x.MasterId, x.AddonId });
                    table.ForeignKey(
                        name: "FK_OrderTickets_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTickets_TicketAddons_AddonId",
                        column: x => x.AddonId,
                        principalTable: "TicketAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTickets_TicketMasters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "TicketMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiredTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GroupSize = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QRCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    TotalPrice = table.Column<float>(type: "real", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_TicketMasters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "TicketMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTicketAddon",
                columns: table => new
                {
                    TicketAddonsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTicketAddon", x => new { x.TicketAddonsId, x.TicketsId });
                    table.ForeignKey(
                        name: "FK_TicketTicketAddon_TicketAddons_TicketAddonsId",
                        column: x => x.TicketAddonsId,
                        principalTable: "TicketAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTicketAddon_Tickets_TicketsId",
                        column: x => x.TicketsId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTickets_AddonId",
                table: "OrderTickets",
                column: "AddonId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTickets_MasterId",
                table: "OrderTickets",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTickets_OrderId",
                table: "OrderTickets",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAddons_EventId",
                table: "TicketAddons",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAddons_MuseumId",
                table: "TicketAddons",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMasters_MuseumId",
                table: "TicketMasters",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMasters_UserId",
                table: "TicketMasters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_MasterId",
                table: "Tickets",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OwnerId",
                table: "Tickets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTicketAddon_TicketsId",
                table: "TicketTicketAddon",
                column: "TicketsId");
        }
    }
}
