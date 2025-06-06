namespace HotelStaffManagement.DataAccess.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
