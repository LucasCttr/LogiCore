using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogiCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPackageCreatorFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_AspNetUsers_ApplicationUserId1",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ApplicationUserId1",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "Packages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "Packages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ApplicationUserId1",
                table: "Packages",
                column: "ApplicationUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_AspNetUsers_ApplicationUserId1",
                table: "Packages",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
