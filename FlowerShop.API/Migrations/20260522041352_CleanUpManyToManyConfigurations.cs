using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpManyToManyConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_product_category_product_categories_category_id",
                table: "product_product_category");

            migrationBuilder.AddForeignKey(
                name: "FK_product_product_category_product_categories_category_id",
                table: "product_product_category",
                column: "category_id",
                principalTable: "product_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_product_category_product_categories_category_id",
                table: "product_product_category");

            migrationBuilder.AddForeignKey(
                name: "FK_product_product_category_product_categories_category_id",
                table: "product_product_category",
                column: "category_id",
                principalTable: "product_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
