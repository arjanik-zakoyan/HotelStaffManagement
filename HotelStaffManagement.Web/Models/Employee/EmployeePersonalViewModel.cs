using HotelStaffManagement.DataAccess.Models;

namespace HotelStaffManagement.Web.Models.Employee
{
    public class EmployeePersonalViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Position { get; set; }
        public decimal SalaryPerHour { get; set; }

        public DateTime CurrentMonth { get; set; }

        public List<EmployeeScheduleDayViewModel> CalendarDays { get; set; } = new();
    }

}
