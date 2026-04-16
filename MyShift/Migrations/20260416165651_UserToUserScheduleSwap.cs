using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class UserToUserScheduleSwap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Users_UserId",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Shifts",
                newName: "UserScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts",
                newName: "IX_Shifts_UserScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_UserSchedules_UserScheduleId",
                table: "Shifts",
                column: "UserScheduleId",
                principalTable: "UserSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_UserSchedules_UserScheduleId",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "UserScheduleId",
                table: "Shifts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Shifts_UserScheduleId",
                table: "Shifts",
                newName: "IX_Shifts_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Users_UserId",
                table: "Shifts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
