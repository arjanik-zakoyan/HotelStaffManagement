namespace HotelStaffManagement.Web.Models.Manager
{
    public class ScheduleDisplayViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DateTime> Dates { get; set; } = new();
        public List<string> Shifts { get; set; } = new();
        public string SelectedPosition { get; set; } = "All";
        public List<string> AllPositions { get; set; } = new();
        public Dictionary<(string Shift, DateTime Date, string Position), List<string>> Schedule { get; set; } = new();
    }
}
