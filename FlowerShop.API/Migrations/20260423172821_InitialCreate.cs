using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowerShop.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(255)", nullable: true),
                    email = table.Column<string>(type: "varchar(255)", nullable: true),
                    password = table.Column<string>(type: "varchar(255)", nullable: true),
                    verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    verified_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    verification_token = table.Column<string>(type: "varchar(255)", nullable: true),
                    remember_token = table.Column<string>(type: "varchar(255)", nullable: true),
                    email_verified_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp(6) without time zone", nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission_role",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission_role", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_permission_role_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_permission_role_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_user",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_user", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_role_user_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_user_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permission_role_permission_id",
                table: "permission_role",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_user_role_id",
                table: "role_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permission_role");

            migrationBuilder.DropTable(
                name: "role_user");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
