using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductStatusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_product_statuses_status_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "product_statuses");

            migrationBuilder.DropIndex(
                name: "IX_products_status_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "products");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "products",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "products");

            migrationBuilder.AddColumn<long>(
                name: "status_id",
                table: "products",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_statuses", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_status_id",
                table: "products",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_statuses_name",
                table: "product_statuses",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_products_product_statuses_status_id",
                table: "products",
                column: "status_id",
                principalTable: "product_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
