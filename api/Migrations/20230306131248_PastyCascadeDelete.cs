using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class PastyCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pasties_pastes_paste_id",
                table: "pasties");

            migrationBuilder.AddForeignKey(
                name: "fk_pasties_pastes_paste_id",
                table: "pasties",
                column: "paste_id",
                principalTable: "pastes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pasties_pastes_paste_id",
                table: "pasties");

            migrationBuilder.AddForeignKey(
                name: "fk_pasties_pastes_paste_id",
                table: "pasties",
                column: "paste_id",
                principalTable: "pastes",
                principalColumn: "id");
        }
    }
}
