using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class UserOAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_contributor",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_supporter",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "provider_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "provider_name",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_users_avatar_id",
                table: "users",
                column: "avatar_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users",
                column: "avatar_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_avatar_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_contributor",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_supporter",
                table: "users");

            migrationBuilder.DropColumn(
                name: "provider_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "provider_name",
                table: "users");
        }
    }
}
