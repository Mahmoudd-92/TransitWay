using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class EditingAlertAndAddingSOSAlertStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FawryReferenceNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DriverIsOkay",
                table: "Alerts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSos",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Alerts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Alerts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedReplacementBus",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SafetyCheckStartedAt",
                table: "Alerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SituationPhotoPath",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FawryReferenceNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DriverIsOkay",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "IsSos",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "NeedReplacementBus",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SafetyCheckStartedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SituationPhotoPath",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Alerts");
        }
    }
}
