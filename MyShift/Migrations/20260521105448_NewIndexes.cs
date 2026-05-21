using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class NewIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Shifts_UserScheduleId",
                table: "Shifts",
                newName: "idx_Shift_UserScheduleId");

            migrationBuilder.CreateIndex(
                name: "uq_ToDoUser_Id",
                table: "Users",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ScheduleTemplate_Type",
                table: "Schedule_Templates",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "idx_Request_Status",
                table: "Requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_Notification_Type",
                table: "Notifications",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_ToDoUser_Id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "idx_ScheduleTemplate_Type",
                table: "Schedule_Templates");

            migrationBuilder.DropIndex(
                name: "idx_Request_Status",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "idx_Notification_Type",
                table: "Notifications");

            migrationBuilder.RenameIndex(
                name: "idx_Shift_UserScheduleId",
                table: "Shifts",
                newName: "IX_Shifts_UserScheduleId");
        }
    }
}
