using HotelStaffManagement.DataAccess.Enums;

namespace HotelStaffManagement.Web.Helpers
{
    public static class PositionHelper
    {
        public static string GetPositionDisplayName(string? position)
        {
            return position switch
            {
                "Receptionist" => "Ընդունարանի աշխատակից",
                "Housekeeper" => "Սենյակավար",
                "Chef" => "Խոհարար",
                "Security" => "Անվտանգություն աշխատակից",
                "Waiter" => "Մատուցող",
                "Bartender" => "Բարմեն",
                "Technician" => "Տեխնիկ",
                _ => position ?? ""
            };
        }

        public static string GetPositionDisplayName(EmployeePosition position)
        {
            return GetPositionDisplayName(position.ToString());
        }
    }


}
