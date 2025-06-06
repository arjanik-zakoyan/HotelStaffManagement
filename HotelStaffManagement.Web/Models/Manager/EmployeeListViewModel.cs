namespace HotelStaffManagement.Web.Models.Manager
{
    public class EmployeeListViewModel
    {
        public List<EmployeeViewModel> Employees { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? Position { get; set; }
    }
}
