using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication.Migrations
{
    /// <inheritdoc />
    public partial class updateOtpTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OtpId",
                table: "Otps",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Otps",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "Otps");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Otps",
                newName: "OtpId");
        }
    }
}
