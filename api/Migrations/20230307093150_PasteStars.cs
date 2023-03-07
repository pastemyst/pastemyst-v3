using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class PasteStars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stars",
                table: "pastes");

            migrationBuilder.CreateTable(
                name: "stars",
                columns: table => new
                {
                    paste_id = table.Column<string>(type: "text", nullable: false),
                    stars_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stars", x => new { x.paste_id, x.stars_id });
                    table.ForeignKey(
                        name: "fk_stars_pastes_paste_id",
                        column: x => x.paste_id,
                        principalTable: "pastes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stars_users_stars_id",
                        column: x => x.stars_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stars_stars_id",
                table: "stars",
                column: "stars_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stars");

            migrationBuilder.AddColumn<int>(
                name: "stars",
                table: "pastes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
