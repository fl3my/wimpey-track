using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WimpeyTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipt_Purchases_PurchaseId",
                table: "Receipt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipt",
                table: "Receipt");

            migrationBuilder.RenameTable(
                name: "Receipt",
                newName: "Receipts");

            migrationBuilder.RenameIndex(
                name: "IX_Receipt_PurchaseId",
                table: "Receipts",
                newName: "IX_Receipts_PurchaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Purchases_PurchaseId",
                table: "Receipts",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Purchases_PurchaseId",
                table: "Receipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts");

            migrationBuilder.RenameTable(
                name: "Receipts",
                newName: "Receipt");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_PurchaseId",
                table: "Receipt",
                newName: "IX_Receipt_PurchaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipt",
                table: "Receipt",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipt_Purchases_PurchaseId",
                table: "Receipt",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");
        }
    }
}
