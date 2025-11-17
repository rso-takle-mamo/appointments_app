using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tenants_users_ownerid",
                table: "tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_users_tenants_tenantid",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenants",
                table: "tenants");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "tenants",
                newName: "Tenants");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "updatedat",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tenantid",
                table: "Users",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "lastname",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "firstname",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_users_username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_users_tenantid",
                table: "Users",
                newName: "IX_Users_TenantId");

            migrationBuilder.RenameColumn(
                name: "updatedat",
                table: "Tenants",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ownerid",
                table: "Tenants",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Tenants",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Tenants",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "businessphone",
                table: "Tenants",
                newName: "BusinessPhone");

            migrationBuilder.RenameColumn(
                name: "businessname",
                table: "Tenants",
                newName: "BusinessName");

            migrationBuilder.RenameColumn(
                name: "businessemail",
                table: "Tenants",
                newName: "BusinessEmail");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Tenants",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tenants",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_tenants_ownerid",
                table: "Tenants",
                newName: "IX_Tenants_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenJti = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ExpiresAt",
                table: "UserSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_TokenJti",
                table: "UserSessions",
                column: "TokenJti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Users_OwnerId",
                table: "Tenants",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Users_OwnerId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "tenants");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updatedat");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "users",
                newName: "tenantid");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "lastname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "firstname");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "users",
                newName: "IX_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_TenantId",
                table: "users",
                newName: "IX_users_tenantid");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "tenants",
                newName: "updatedat");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "tenants",
                newName: "ownerid");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "tenants",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tenants",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "BusinessPhone",
                table: "tenants",
                newName: "businessphone");

            migrationBuilder.RenameColumn(
                name: "BusinessName",
                table: "tenants",
                newName: "businessname");

            migrationBuilder.RenameColumn(
                name: "BusinessEmail",
                table: "tenants",
                newName: "businessemail");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "tenants",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tenants",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_OwnerId",
                table: "tenants",
                newName: "IX_tenants_ownerid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenants",
                table: "tenants",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tenants_users_ownerid",
                table: "tenants",
                column: "ownerid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_tenants_tenantid",
                table: "users",
                column: "tenantid",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
