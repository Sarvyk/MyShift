using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class updateNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_userId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Notifications",
                newName: "requestId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_userId",
                table: "Notifications",
                newName: "IX_Notifications_requestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Requests_requestId",
                table: "Notifications",
                column: "requestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Requests_requestId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "requestId",
                table: "Notifications",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_requestId",
                table: "Notifications",
                newName: "IX_Notifications_userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_userId",
                table: "Notifications",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
