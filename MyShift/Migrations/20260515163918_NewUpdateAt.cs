using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdateAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Requests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Requests");
        }
    }
}
