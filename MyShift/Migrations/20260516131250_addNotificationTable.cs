using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class addNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToDoUser_ScheduleTemplates");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsNotified = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_userId",
                table: "Notifications",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.CreateTable(
                name: "ToDoUser_ScheduleTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    Is_Cancelled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDoUser_ScheduleTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToDoUser_ScheduleTemplates_Schedule_Templates_ScheduleTemplateId",
                        column: x => x.ScheduleTemplateId,
                        principalTable: "Schedule_Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToDoUser_ScheduleTemplates_UserSchedules_FirstScheduleId",
                        column: x => x.FirstScheduleId,
                        principalTable: "UserSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToDoUser_ScheduleTemplates_FirstScheduleId",
                table: "ToDoUser_ScheduleTemplates",
                column: "FirstScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoUser_ScheduleTemplates_ScheduleTemplateId",
                table: "ToDoUser_ScheduleTemplates",
                column: "ScheduleTemplateId");
        }
    }
}
