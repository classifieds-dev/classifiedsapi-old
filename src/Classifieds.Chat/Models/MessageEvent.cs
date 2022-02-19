using Shared.Models;

namespace Chat.Models
{
    public class MessageEvent
    {
        public string Action { get; set; }
        public ChatMessage Message { get; set; }
    }
}