using System;

namespace Shared.Models
{
    public class ChatMessage
    {
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
