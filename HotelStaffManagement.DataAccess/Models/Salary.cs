namespace HotelStaffManagement.DataAccess.Models
{
    public class Salary
    {
        public int SalaryID { get; set; }
        public int EmployeeID { get; set; }
        public string Month { get; set; } = null!;
        public decimal TotalSalaryAmount { get; set; }
        public decimal RegularHours { get; set; }
        public decimal NightHours { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
