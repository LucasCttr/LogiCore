using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeShipmentPackageDeleteBehaviorToSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages",
                column: "CurrentShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Shipments_CurrentShipmentId",
                table: "Packages",
                column: "CurrentShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
