using Shared.Models;

namespace Chat.Mappings
{

    public class ChatMappings : Cassandra.Mapping.Mappings
    {
        public ChatMappings()
        {
            // Define mappings in the constructor of your class
            // that inherits from Mappings
            For<ChatMessage>()
               .TableName("chatmessages")
               .PartitionKey(m => m.SenderId, m => m.RecipientId)
               .ClusteringKey(m => m.CreatedAt)
               .Column(c => c.CreatedAt, c => c.WithName("createdat"))
               .Column(c => c.RecipientId, c => c.WithName("recipientid"))
               .Column(c => c.SenderId, c => c.WithName("senderid"))
               .Column(c => c.Message, c => c.WithName("message"));
            For<ChatConnection>()
               .TableName("chatconnections")
               .PartitionKey(c => c.ConnId, c => c.UserId)
               .ClusteringKey(m => m.CreatedAt)
               .Column(c => c.ConnId, c => c.WithName("connid"))
               .Column(c => c.UserId, c => c.WithName("userid"))
               .Column(c => c.CreatedAt, c => c.WithName("createdat"));
            For<ChatConversation>()
               .TableName("chatconversations")
               .PartitionKey(c => c.UserId)
               .ClusteringKey(c => c.RecipientId)
               .Column(c => c.UserId, c => c.WithName("userid"))
               .Column(c => c.RecipientId, c => c.WithName("recipientid"))
               .Column(c => c.RecipientLabel, c => c.WithName("recipientlabel"));
        }
    }

}