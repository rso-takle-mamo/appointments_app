using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsToUpperCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Categories_category_id",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "updatedat",
                table: "Services",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "Services",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Services",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Services",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Services",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Services",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Services",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "duration_minutes",
                table: "Services",
                newName: "DurationMinutes");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Services",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Services_category_id",
                table: "Services",
                newName: "IX_Services_CategoryId");

            migrationBuilder.RenameColumn(
                name: "updatedat",
                table: "Categories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_name",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Categories_CategoryId",
                table: "Services",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Categories_CategoryId",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Services",
                newName: "updatedat");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Services",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Services",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Services",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Services",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Services",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Services",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "Services",
                newName: "duration_minutes");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Services",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_Services_CategoryId",
                table: "Services",
                newName: "IX_Services_category_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Categories",
                newName: "updatedat");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categories",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Categories",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                newName: "IX_Categories_name");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Categories_category_id",
                table: "Services",
                column: "category_id",
                principalTable: "Categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
