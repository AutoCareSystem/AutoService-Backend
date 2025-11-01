using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Management_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicePackageID",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Appointments",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServicePackageID",
                table: "Appointments",
                column: "ServicePackageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServicePackages_ServicePackageID",
                table: "Appointments",
                column: "ServicePackageID",
                principalTable: "ServicePackages",
                principalColumn: "ServicePackageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServicePackages_ServicePackageID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServicePackageID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ServicePackageID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Appointments");
        }
    }
}
