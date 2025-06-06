namespace HotelStaffManagement.DataAccess.Models
{ 
    public class Message
    {
        public int MessageID { get; set; }
        public int? SenderID { get; set; }
        public int? ReceiverID { get; set; }
        public string Text { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}
