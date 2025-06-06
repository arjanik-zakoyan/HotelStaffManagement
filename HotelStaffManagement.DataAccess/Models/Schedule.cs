namespace HotelStaffManagement.DataAccess.Models
{
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int? ManagerID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public User? Manager { get; set; }
        public Employee Employee { get; set; } = null!;
    }

}
