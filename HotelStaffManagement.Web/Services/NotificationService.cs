using HotelStaffManagement.DataAccess.Models;
using HotelStaffManagement.DataAccess;

namespace HotelStaffManagement.Web.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(int userId, string title, string message, string? type = null)
        {
            var notification = new Notification
            {
                UserID = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now
            }; 

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
