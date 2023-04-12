using Microsoft.EntityFrameworkCore.Migrations;
using pastemyst.Models;

#nullable disable

namespace pastemyst.Migrations
{
    /// <inheritdoc />
    public partial class RegisteredEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"alter table pastes add column expires_in2 expires_in not null");
            migrationBuilder.Sql(@"update pastes set expires_in2 = 'never'::expires_in");
            migrationBuilder.Sql(@"alter table pastes drop column expires_in");
            migrationBuilder.Sql(@"alter table pastes rename column expires_in2 to expires_in");

            migrationBuilder.Sql(@"alter table action_logs add column type2 action_log_type not null");
            migrationBuilder.Sql(@"update action_logs set type2 = 'paste_created'::action_log_type");
            migrationBuilder.Sql(@"alter table action_logs drop column type");
            migrationBuilder.Sql(@"alter table action_logs rename column type2 to type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"alter table pastes add column expires_in2 integer");
            migrationBuilder.Sql(@"update pastes set expires_in2 = 0");
            migrationBuilder.Sql(@"alter table pastes drop column expires_in");
            migrationBuilder.Sql(@"alter table pastes rename column expires_in2 to expires_in");

            migrationBuilder.Sql(@"alter table action_logs add column type2 integer");
            migrationBuilder.Sql(@"update action_logs set type2 = 0");
            migrationBuilder.Sql(@"alter table action_logs drop column type");
            migrationBuilder.Sql(@"alter table action_logs rename column type2 to type");
        }
    }
}
