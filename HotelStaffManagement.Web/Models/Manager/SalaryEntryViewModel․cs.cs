namespace HotelStaffManagement.Web.Models.Manager
{
    public class SalaryEntryViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public decimal RegularHours { get; set; }
        public decimal NightHours { get; set; }
        public decimal TotalSalaryAmount { get; set; }
    }
}
