using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HotelStaffManagement.Web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int receiverId, string message, string sentAt)
        {
            var senderId = int.Parse(Context.UserIdentifier!);

            // Sending To Reciver New Message
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message, sentAt, false);

            // Sending To Sender His Message(read)
            await Clients.User(senderId.ToString()).SendAsync("ReceiveOwnMessage", receiverId, message, sentAt, true);
        }
    }
}
