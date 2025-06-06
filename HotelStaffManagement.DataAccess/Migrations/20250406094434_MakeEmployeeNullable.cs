using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelStaffManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployeeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_ManagerID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_EmployeeID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ManagerID",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_ManagerID",
                table: "Schedules",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_EmployeeID",
                table: "Tasks",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ManagerID",
                table: "Tasks",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_ManagerID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_EmployeeID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ManagerID",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_EmployeeID",
                table: "Schedules",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_ManagerID",
                table: "Schedules",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_EmployeeID",
                table: "Tasks",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ManagerID",
                table: "Tasks",
                column: "ManagerID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
