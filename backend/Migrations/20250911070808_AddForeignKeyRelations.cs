using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_events_users_user_id",
                table: "events",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_event_participants_events_event_id",
                table: "event_participants",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_event_participants_users_user_id",
                table: "event_participants",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FKs first (reverse order)
            migrationBuilder.DropForeignKey(
                name: "FK_event_participants_users_user_id",
                table: "event_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_event_participants_events_event_id",
                table: "event_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_events_users_user_id",
                table: "events");
        }
    }
}
