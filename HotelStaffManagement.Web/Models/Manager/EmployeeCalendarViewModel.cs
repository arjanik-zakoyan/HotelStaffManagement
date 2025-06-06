namespace HotelStaffManagement.Web.Models.Manager
{
    public class EmployeeCalendarViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Position { get; set; }
        public decimal SalaryPerHour { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<CalendarDayViewModel> CalendarDays { get; set; } = new();
        public List<List<CalendarDayViewModel>> Weeks => CalendarDays
            .OrderBy(d => d.Date)
            .Chunk(7)
            .Select(c => c.ToList())
            .ToList();
    }
}
