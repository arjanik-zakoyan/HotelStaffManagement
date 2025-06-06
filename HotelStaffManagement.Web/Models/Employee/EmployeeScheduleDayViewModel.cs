namespace HotelStaffManagement.Web.Models.Employee
{
    public class EmployeeScheduleDayViewModel
    {
        public DateTime Date { get; set; }
        public List<string> ShiftTimes { get; set; } = new();
    }

}
