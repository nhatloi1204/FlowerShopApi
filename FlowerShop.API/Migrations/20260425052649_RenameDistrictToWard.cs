using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameDistrictToWard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customer_addresses_districts_DistrictId1",
                table: "customer_addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_customer_addresses_districts_district_id",
                table: "customer_addresses");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.RenameColumn(
                name: "district_id",
                table: "customer_addresses",
                newName: "ward_id");

            migrationBuilder.RenameColumn(
                name: "DistrictId1",
                table: "customer_addresses",
                newName: "WardId1");

            migrationBuilder.RenameIndex(
                name: "IX_customer_addresses_DistrictId1",
                table: "customer_addresses",
                newName: "IX_customer_addresses_WardId1");

            migrationBuilder.RenameIndex(
                name: "IX_customer_addresses_district_id",
                table: "customer_addresses",
                newName: "IX_customer_addresses_ward_id");

            migrationBuilder.CreateTable(
                name: "wards",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    province_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wards", x => x.id);
                    table.ForeignKey(
                        name: "FK_wards_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wards_province_id",
                table: "wards",
                column: "province_id");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_addresses_wards_WardId1",
                table: "customer_addresses",
                column: "WardId1",
                principalTable: "wards",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_addresses_wards_ward_id",
                table: "customer_addresses",
                column: "ward_id",
                principalTable: "wards",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customer_addresses_wards_WardId1",
                table: "customer_addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_customer_addresses_wards_ward_id",
                table: "customer_addresses");

            migrationBuilder.DropTable(
                name: "wards");

            migrationBuilder.RenameColumn(
                name: "ward_id",
                table: "customer_addresses",
                newName: "district_id");

            migrationBuilder.RenameColumn(
                name: "WardId1",
                table: "customer_addresses",
                newName: "DistrictId1");

            migrationBuilder.RenameIndex(
                name: "IX_customer_addresses_WardId1",
                table: "customer_addresses",
                newName: "IX_customer_addresses_DistrictId1");

            migrationBuilder.RenameIndex(
                name: "IX_customer_addresses_ward_id",
                table: "customer_addresses",
                newName: "IX_customer_addresses_district_id");

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    province_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_districts", x => x.id);
                    table.ForeignKey(
                        name: "FK_districts_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_districts_province_id",
                table: "districts",
                column: "province_id");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_addresses_districts_DistrictId1",
                table: "customer_addresses",
                column: "DistrictId1",
                principalTable: "districts",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_addresses_districts_district_id",
                table: "customer_addresses",
                column: "district_id",
                principalTable: "districts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
