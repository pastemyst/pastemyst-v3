using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class AddPasteExpiredActionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:action_log_type", "paste_created,paste_deleted,paste_expired,user_created,user_deleted")
                .Annotation("Npgsql:Enum:expires_in", "never,one_hour,two_hours,ten_hours,one_day,two_days,one_week,one_month,one_year")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:action_log_type", "paste_created,paste_deleted,user_created")
                .OldAnnotation("Npgsql:Enum:expires_in", "never,one_hour,two_hours,ten_hours,one_day,two_days,one_week,one_month,one_year")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:action_log_type", "paste_created,paste_deleted,user_created")
                .Annotation("Npgsql:Enum:expires_in", "never,one_hour,two_hours,ten_hours,one_day,two_days,one_week,one_month,one_year")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:action_log_type", "paste_created,paste_deleted,paste_expired,user_created,user_deleted")
                .OldAnnotation("Npgsql:Enum:expires_in", "never,one_hour,two_hours,ten_hours,one_day,two_days,one_week,one_month,one_year")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }
    }
}
