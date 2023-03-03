using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class ImageContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "images",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "images");
        }
    }
}
