using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginLocationToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginLocationId",
                table: "Shipments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginLocationId",
                table: "Shipments");
        }
    }
}
