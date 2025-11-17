using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActiveInUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
