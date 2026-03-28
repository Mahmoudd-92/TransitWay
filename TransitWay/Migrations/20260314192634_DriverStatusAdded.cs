using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class DriverStatusAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Drivers");

            migrationBuilder.AlterColumn<int>(
                name: "BusId",
                table: "Drivers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers",
                column: "BusId",
                unique: true,
                filter: "[BusId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Drivers");

            migrationBuilder.AlterColumn<int>(
                name: "BusId",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers",
                column: "BusId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
