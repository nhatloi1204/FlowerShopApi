using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_category_categories_category_id",
                table: "product_category");

            migrationBuilder.DropForeignKey(
                name: "FK_product_category_products_product_id",
                table: "product_category");

            migrationBuilder.DropForeignKey(
                name: "FK_products_product_statuses_product_status_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "IX_products_product_status_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_sku",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_product_tags_slug",
                table: "product_tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_category",
                table: "product_category");

            migrationBuilder.DropIndex(
                name: "IX_product_category_category_id",
                table: "product_category");

            migrationBuilder.DropColumn(
                name: "cost_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "discount_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "products");

            migrationBuilder.DropColumn(
                name: "main_image",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_status_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "products");

            migrationBuilder.DropColumn(
                name: "sku",
                table: "products");

            migrationBuilder.DropColumn(
                name: "view_count",
                table: "products");

            migrationBuilder.DropColumn(
                name: "description",
                table: "product_tags");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "product_tags");

            migrationBuilder.DropColumn(
                name: "description",
                table: "product_statuses");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "product_statuses");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "product_category");

            migrationBuilder.RenameTable(
                name: "product_category",
                newName: "product_categories");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "product_categories",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "slug",
                table: "products",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "products",
                type: "numeric(15,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "products",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<long>(
                name: "company_id",
                table: "products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_max",
                table: "products",
                type: "numeric(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_min",
                table: "products",
                type: "numeric(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "status_id",
                table: "products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock_quantity",
                table: "products",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product_tags",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product_statuses",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "product_statuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "product_categories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "product_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "product_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "product_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "product_categories",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "product_categories",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "product_categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_categories",
                table: "product_categories",
                column: "id");

            migrationBuilder.CreateTable(
                name: "product_product_category",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_product_category", x => new { x.product_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_product_product_category_product_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "product_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_product_category_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_name",
                table: "products",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_status_id",
                table: "products",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_statuses_name",
                table: "product_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_slug",
                table: "product_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_product_category_category_id",
                table: "product_product_category",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_product_statuses_status_id",
                table: "products",
                column: "status_id",
                principalTable: "product_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_product_statuses_status_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "product_product_category");

            migrationBuilder.DropIndex(
                name: "IX_products_name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_status_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_product_statuses_name",
                table: "product_statuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_categories",
                table: "product_categories");

            migrationBuilder.DropIndex(
                name: "IX_product_categories_slug",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "price_max",
                table: "products");

            migrationBuilder.DropColumn(
                name: "price_min",
                table: "products");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "stock_quantity",
                table: "products");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "product_statuses");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "description",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "name",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "product_categories");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "product_categories");

            migrationBuilder.RenameTable(
                name: "product_categories",
                newName: "product_category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "product_category",
                newName: "category_id");

            migrationBuilder.AlterColumn<string>(
                name: "slug",
                table: "products",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "products",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(15,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "products",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cost_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "main_image",
                table: "products",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "product_status_id",
                table: "products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "sku",
                table: "products",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "view_count",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product_tags",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "product_tags",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "product_tags",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product_statuses",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "product_statuses",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "product_statuses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<long>(
                name: "category_id",
                table: "product_category",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "product_id",
                table: "product_category",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_category",
                table: "product_category",
                columns: new[] { "product_id", "category_id" });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    image = table.Column<string>(type: "varchar(255)", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    slug = table.Column<string>(type: "varchar(255)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_product_status_id",
                table: "products",
                column: "product_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_tags_slug",
                table: "product_tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_category_category_id",
                table: "product_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_category_categories_category_id",
                table: "product_category",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_category_products_product_id",
                table: "product_category",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_products_product_statuses_product_status_id",
                table: "products",
                column: "product_status_id",
                principalTable: "product_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
