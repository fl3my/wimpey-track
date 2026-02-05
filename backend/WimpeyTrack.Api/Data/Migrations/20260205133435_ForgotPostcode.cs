using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WimpeyTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ForgotPostcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomePostcode",
                table: "Profiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomePostcode",
                table: "Profiles");
        }
    }
}
