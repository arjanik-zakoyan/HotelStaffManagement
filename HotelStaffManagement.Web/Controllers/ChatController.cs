using HotelStaffManagement.DataAccess;
using HotelStaffManagement.DataAccess.Models;
using HotelStaffManagement.Web.Dtos;
using HotelStaffManagement.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HotelStaffManagement.Web.Controllers
{
    [Authorize(Roles = "Manager,Employee")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetUsers(string role)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var users = _context.Users
                .Where(u => u.Role == role && u.UserID != currentUserId)
                .Join(_context.Employees,
                      u => u.UserID,
                      e => e.UserID,
                      (u, e) => new
                      {
                          u.UserID,
                          u.Username,
                          e.FullName,
                          e.Position,
                          LastMessageTime = _context.Messages
                              .Where(m => (m.SenderID == currentUserId && m.ReceiverID == u.UserID) ||
                                          (m.SenderID == u.UserID && m.ReceiverID == currentUserId))
                              .OrderByDescending(m => m.SentAt)
                              .Select(m => (DateTime?)m.SentAt)
                              .FirstOrDefault(),
                          UnreadCount = _context.Messages.Count(m => m.SenderID == u.UserID && m.ReceiverID == currentUserId && !m.IsRead)
                      })
                .OrderByDescending(u => u.LastMessageTime ?? DateTime.MinValue)
                .ToList();
            return Json(users);
        }

        [HttpGet]
        public IActionResult GetMessages(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var messages = _context.Messages
                .Where(m => (m.SenderID == currentUserId && m.ReceiverID == userId) ||
                            (m.SenderID == userId && m.ReceiverID == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Text,
                    IsMine = m.SenderID == currentUserId,
                    SentAt = m.SentAt.ToString("HH:mm dd.MM.yyyy"),
                    m.IsRead,
                    m.MessageID
                })
                .ToList();

            return Json(messages);
        }

        [HttpGet]
        public IActionResult GetUnreadCounts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = _context.Messages
                .Where(m => m.ReceiverID == userId && !m.IsRead)
                .GroupBy(m => m.SenderID)
                .Select(g => new
                {
                    userId = g.Key,
                    count = g.Count()
                })
                .ToList();

            return Json(result);
        }

        [HttpPost]
        public IActionResult MarkAsRead(int senderId)
        {
            var receiverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var messages = _context.Messages
                .Where(m => m.SenderID == senderId && m.ReceiverID == receiverId && !m.IsRead)
                .ToList();

            foreach (var msg in messages)
            {
                msg.IsRead = true;
            }

            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text) || dto.ReceiverID <= 0)
                return BadRequest("Հաղորդագրությունը կամ ստացողը բացակայում է։");

            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var message = new Message
            {
                SenderID = senderId,
                ReceiverID = dto.ReceiverID,
                Text = dto.Text.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };
            if (message.Text.Length > 255)
            {
                return BadRequest("Նամակի առավելագույն չափը կարող է լինել 255 սիմվոլ");
            }
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(dto.ReceiverID.ToString()).SendAsync(
                "ReceiveMessage",
                senderId,
                message.Text,
                message.SentAt.ToString("HH:mm dd.MM.yyyy"),
                false
            );

            await _hubContext.Clients.User(senderId.ToString()).SendAsync(
                "ReceiveOwnMessage",
                dto.ReceiverID,
                message.Text,
                message.SentAt.ToString("HH:mm dd.MM.yyyy"),
                true
            );

            return Ok(new
            {
                success = true,
                messageId = message.MessageID,
                sentAt = message.SentAt.ToString("HH:mm dd.MM.yyyy")
            });
        }

    }
}
