using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WimpeyTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    StaffNumber = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessUnit = table.Column<string>(type: "TEXT", nullable: false),
                    DepartmentSiteName = table.Column<string>(type: "TEXT", nullable: false),
                    VehicleFuelType = table.Column<string>(type: "TEXT", nullable: false),
                    VehicleEngineSize = table.Column<int>(type: "INTEGER", nullable: false),
                    VehicleRegistration = table.Column<string>(type: "TEXT", nullable: false),
                    VehicleMake = table.Column<string>(type: "TEXT", nullable: false),
                    HomeLocationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_Locations_HomeLocationId",
                        column: x => x.HomeLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_HomeLocationId",
                table: "Profiles",
                column: "HomeLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
