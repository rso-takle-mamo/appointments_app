using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Database.Migrations
{
    /// <inheritdoc />
    internal partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ownerid = table.Column<Guid>(type: "uuid", nullable: false),
                    businessname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    businessemail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    businessphone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    tenantid = table.Column<Guid>(type: "uuid", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_tenants_tenantid",
                        column: x => x.tenantid,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_ownerid",
                table: "tenants",
                column: "ownerid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_tenantid",
                table: "users",
                column: "tenantid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tenants_users_ownerid",
                table: "tenants",
                column: "ownerid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tenants_users_ownerid",
                table: "tenants");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
