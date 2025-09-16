using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FieldNameChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "timezone",
                table: "events",
                newName: "time_zone");

            migrationBuilder.RenameColumn(
                name: "event_date",
                table: "events",
                newName: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "time_zone",
                table: "events",
                newName: "timezone");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "events",
                newName: "event_date");
        }
    }
}
