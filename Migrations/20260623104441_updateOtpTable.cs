using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication.Migrations
{
    /// <inheritdoc />
    public partial class updateOtpTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Otps",
                newName: "OtpId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Otps",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Otps");

            migrationBuilder.RenameColumn(
                name: "OtpId",
                table: "Otps",
                newName: "Id");
        }
    }
}
