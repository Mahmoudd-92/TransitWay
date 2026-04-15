using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class EditRouteQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteQrs_Routes_RouteId",
                table: "RouteQrs");

            migrationBuilder.AddColumn<int>(
                name: "BusId",
                table: "RouteQrs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RouteQrs_BusId",
                table: "RouteQrs",
                column: "BusId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteQrs_Buses_BusId",
                table: "RouteQrs",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteQrs_Routes_RouteId",
                table: "RouteQrs",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteQrs_Buses_BusId",
                table: "RouteQrs");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteQrs_Routes_RouteId",
                table: "RouteQrs");

            migrationBuilder.DropIndex(
                name: "IX_RouteQrs_BusId",
                table: "RouteQrs");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "RouteQrs");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteQrs_Routes_RouteId",
                table: "RouteQrs",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
