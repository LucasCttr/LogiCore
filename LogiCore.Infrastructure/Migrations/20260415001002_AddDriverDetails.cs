using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LicenseType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LicenseExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InsuranceExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedVehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverDetails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverDetails_Vehicle_AssignedVehicleId",
                        column: x => x.AssignedVehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverDetails_AssignedVehicleId",
                table: "DriverDetails",
                column: "AssignedVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDetails_UserId",
                table: "DriverDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverDetails");
        }
    }
}
