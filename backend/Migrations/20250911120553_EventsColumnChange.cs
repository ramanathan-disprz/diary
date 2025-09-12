using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class EventsColumnChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old datetime -> convert to time
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "start_time",
                table: "events",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "end_time",
                table: "events",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            // Add new event_date column
            migrationBuilder.AddColumn<DateOnly>(
                name: "event_date",
                table: "events",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2025, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove event_date
            migrationBuilder.DropColumn(
                name: "event_date",
                table: "events");

            // Revert start_time and end_time back to datetime
            migrationBuilder.AlterColumn<DateTime>(
                name: "start_time",
                table: "events",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_time",
                table: "events",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");
        }
    }
}