using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShift.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Shifts RENAME COLUMN CancelReason TO CancelReason_old");
            migrationBuilder.Sql("ALTER TABLE Shifts ADD COLUMN CancelReason TEXT");
            migrationBuilder.Sql("UPDATE Shifts SET CancelReason = CancelReason_old");
            migrationBuilder.Sql("ALTER TABLE Shifts DROP COLUMN CancelReason_old");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
