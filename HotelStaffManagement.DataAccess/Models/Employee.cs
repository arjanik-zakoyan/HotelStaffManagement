namespace HotelStaffManagement.DataAccess.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string Position { get; set; } = null!;
        public decimal SalaryPerHour { get; set; }
        public User User { get; set; } = null!;
    }
}
