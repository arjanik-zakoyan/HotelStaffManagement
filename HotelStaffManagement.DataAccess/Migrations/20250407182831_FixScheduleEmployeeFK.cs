using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelStaffManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixScheduleEmployeeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Employees_EmployeeID",
                table: "Schedules",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Employees_EmployeeID",
                table: "Schedules");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
