using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageStatusHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageStatusHistories");
        }
    }
}
