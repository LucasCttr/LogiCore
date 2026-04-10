using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCurrentShipmentIdTypeManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old integer column and create a new uuid column
            migrationBuilder.DropColumn(
                name: "CurrentShipmentId",
                table: "Packages");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentShipmentId",
                table: "Packages",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: drop uuid column and recreate integer column
            migrationBuilder.DropColumn(
                name: "CurrentShipmentId",
                table: "Packages");

            migrationBuilder.AddColumn<int>(
                name: "CurrentShipmentId",
                table: "Packages",
                type: "integer",
                nullable: true);
        }
    }
}
