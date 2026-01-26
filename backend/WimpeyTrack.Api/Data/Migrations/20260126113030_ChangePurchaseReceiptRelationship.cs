using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WimpeyTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangePurchaseReceiptRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Purchases_PurchaseId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_PurchaseId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Receipts");

            migrationBuilder.AddColumn<int>(
                name: "ReceiptId",
                table: "Purchases",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ReceiptId",
                table: "Purchases",
                column: "ReceiptId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Receipts_ReceiptId",
                table: "Purchases",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Receipts_ReceiptId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_ReceiptId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "Purchases");

            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "Receipts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PurchaseId",
                table: "Receipts",
                column: "PurchaseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Purchases_PurchaseId",
                table: "Receipts",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");
        }
    }
}
