using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Shipments_ShipmentId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ShipmentId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "Packages");

            migrationBuilder.AddColumn<int>(
                name: "ShipmentType",
                table: "Shipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Update ShipmentType based on existing locations:
            // - Transfer (1): has both OriginLocationId AND DestinationLocationId
            // - Pickup (0): has only OriginLocationId
            // - LastMile (2): has only DestinationLocationId
            migrationBuilder.Sql(@"
                UPDATE ""Shipments"" 
                SET ""ShipmentType"" = 1 
                WHERE ""OriginLocationId"" IS NOT NULL 
                  AND ""DestinationLocationId"" IS NOT NULL;

                UPDATE ""Shipments"" 
                SET ""ShipmentType"" = 2 
                WHERE ""OriginLocationId"" IS NULL 
                  AND ""DestinationLocationId"" IS NOT NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_CurrentShipmentId",
                table: "Packages",
                column: "CurrentShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages",
                column: "CurrentShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_CurrentShipmentId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ShipmentType",
                table: "Shipments");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "Packages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ShipmentId",
                table: "Packages",
                column: "ShipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Shipments_ShipmentId",
                table: "Packages",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id");
        }
    }
}
