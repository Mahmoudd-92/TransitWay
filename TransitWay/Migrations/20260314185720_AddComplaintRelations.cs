using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Complaints");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Complaints",
                newName: "ResultImagePath");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Complaints",
                newName: "OriginalImagePath");

            migrationBuilder.AddColumn<int>(
                name: "BusId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ProblemDetected",
                table: "Complaints",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BusId",
                table: "Complaints",
                column: "BusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Buses_BusId",
                table: "Complaints",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Buses_BusId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_BusId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ProblemDetected",
                table: "Complaints");

            migrationBuilder.RenameColumn(
                name: "ResultImagePath",
                table: "Complaints",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "OriginalImagePath",
                table: "Complaints",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
