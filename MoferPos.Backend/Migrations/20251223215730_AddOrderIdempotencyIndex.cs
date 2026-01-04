using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoferPos.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIdempotencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrganizationId_LocationId_ExternalOrderRef",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrganizationId_LocationId_ExternalOrderRef",
                table: "Orders",
                columns: new[] { "OrganizationId", "LocationId", "ExternalOrderRef" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrganizationId_LocationId_ExternalOrderRef",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrganizationId_LocationId_ExternalOrderRef",
                table: "Orders",
                columns: new[] { "OrganizationId", "LocationId", "ExternalOrderRef" });
        }
    }
}
