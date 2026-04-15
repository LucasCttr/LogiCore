using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhancePackageHistoryWithContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "PackageStatusHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PackageStatusHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentId",
                table: "PackageStatusHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PackageStatusHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "PackageStatusHistories");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PackageStatusHistories");

            migrationBuilder.DropColumn(
                name: "ShipmentId",
                table: "PackageStatusHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PackageStatusHistories");
        }
    }
}
