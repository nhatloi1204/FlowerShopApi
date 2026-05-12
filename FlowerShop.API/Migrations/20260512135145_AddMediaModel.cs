using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_type = table.Column<string>(type: "varchar(255)", nullable: false),
                    model_id = table.Column<long>(type: "bigint", nullable: false),
                    uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    collection_name = table.Column<string>(type: "varchar(255)", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    file_name = table.Column<string>(type: "varchar(255)", nullable: false),
                    mime_type = table.Column<string>(type: "varchar(255)", nullable: true),
                    disk = table.Column<string>(type: "varchar(255)", nullable: false),
                    conversions_disk = table.Column<string>(type: "varchar(255)", nullable: true),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    manipulations = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    custom_properties = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    generated_conversions = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    responsive_images = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    order_column = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_media_model",
                table: "media",
                columns: new[] { "model_type", "model_id" });

            migrationBuilder.CreateIndex(
                name: "idx_media_order_column",
                table: "media",
                column: "order_column");

            migrationBuilder.CreateIndex(
                name: "IX_media_uuid",
                table: "media",
                column: "uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media");
        }
    }
}
