using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class EditsssAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Admins",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Admins",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Admins",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Admins",
                newName: "Name");
        }
    }
}
