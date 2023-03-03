using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class Paste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "citext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext");

            migrationBuilder.AlterColumn<string>(
                name: "provider_name",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "provider_id",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "avatar_id",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<byte[]>(
                name: "bytes",
                table: "images",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.CreateTable(
                name: "pastes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_in = table.Column<int>(type: "integer", nullable: false),
                    deletes_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: true),
                    @private = table.Column<bool>(name: "private", type: "boolean", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pastes", x => x.id);
                    table.ForeignKey(
                        name: "fk_pastes_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pasties",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tile = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true),
                    paste_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pasties", x => x.id);
                    table.ForeignKey(
                        name: "fk_pasties_pastes_paste_id",
                        column: x => x.paste_id,
                        principalTable: "pastes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_pastes_owner_id",
                table: "pastes",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_pasties_paste_id",
                table: "pasties",
                column: "paste_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users",
                column: "avatar_id",
                principalTable: "images",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "pasties");

            migrationBuilder.DropTable(
                name: "pastes");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "citext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "citext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "provider_name",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "provider_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "avatar_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "content_type",
                table: "images",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "bytes",
                table: "images",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_users_images_avatar_id",
                table: "users",
                column: "avatar_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
