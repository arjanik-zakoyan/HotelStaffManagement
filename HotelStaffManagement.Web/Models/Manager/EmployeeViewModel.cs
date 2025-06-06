using HotelStaffManagement.Web.Helpers;

namespace HotelStaffManagement.Web.Models.Manager
{
    public class EmployeeViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public decimal SalaryPerHour { get; set; }
        public string DisplayPosition => PositionHelper.GetPositionDisplayName(Position);

    }

}
