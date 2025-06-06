namespace HotelStaffManagement.Web.Models.Manager
{
    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool HasSchedule { get; set; }
        public string? ShiftTime { get; set; }
    }
}
