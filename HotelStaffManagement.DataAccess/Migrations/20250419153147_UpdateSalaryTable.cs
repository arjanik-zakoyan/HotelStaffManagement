using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelStaffManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalaryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CalculatedSalary",
                table: "Salaries",
                newName: "TotalSalaryAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "NightHours",
                table: "Salaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RegularHours",
                table: "Salaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NightHours",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "RegularHours",
                table: "Salaries");

            migrationBuilder.RenameColumn(
                name: "TotalSalaryAmount",
                table: "Salaries",
                newName: "CalculatedSalary");
        }
    }
}
