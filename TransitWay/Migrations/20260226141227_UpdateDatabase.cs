using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Zones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdmin",
                table: "Stations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Routes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusId",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Buses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdmin",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_CreatedByAdminId",
                table: "Zones",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_CreatedByAdmin",
                table: "Stations",
                column: "CreatedByAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_CreatedByAdminId",
                table: "Routes",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers",
                column: "BusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedByAdminId",
                table: "Drivers",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_CreatedByAdminId",
                table: "Buses",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AdminId",
                table: "Alerts",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedByAdmin",
                table: "Alerts",
                column: "CreatedByAdmin");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Admins_AdminId",
                table: "Alerts",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Admins_CreatedByAdmin",
                table: "Alerts",
                column: "CreatedByAdmin",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Admins_CreatedByAdminId",
                table: "Buses",
                column: "CreatedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Admins_CreatedByAdminId",
                table: "Drivers",
                column: "CreatedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Admins_CreatedByAdminId",
                table: "Routes",
                column: "CreatedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Stations_Admins_CreatedByAdmin",
                table: "Stations",
                column: "CreatedByAdmin",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Zones_Admins_CreatedByAdminId",
                table: "Zones",
                column: "CreatedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Admins_AdminId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Admins_CreatedByAdmin",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Admins_CreatedByAdminId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Admins_CreatedByAdminId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Buses_BusId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Admins_CreatedByAdminId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Stations_Admins_CreatedByAdmin",
                table: "Stations");

            migrationBuilder.DropForeignKey(
                name: "FK_Zones_Admins_CreatedByAdminId",
                table: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_Zones_CreatedByAdminId",
                table: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_Stations_CreatedByAdmin",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Routes_CreatedByAdminId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_BusId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CreatedByAdminId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Buses_CreatedByAdminId",
                table: "Buses");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AdminId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_CreatedByAdmin",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "CreatedByAdmin",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedByAdmin",
                table: "Alerts");
        }
    }
}
