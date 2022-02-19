using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json.Serialization;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

namespace Chat.Repositories
{
    public class ChatRepository
    {
        private readonly IMapper _mapper;
        private readonly IAmazonApiGatewayManagementApi _apiClient;
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;

        public ChatRepository(ICluster cluster, IAmazonCognitoIdentityProvider cognitoClient)
        {
            _cognitoClient = cognitoClient;
            _apiClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = "https://5b1dxqpji3.execute-api.us-east-1.amazonaws.com/dev"
            });
            try
            {
                var session = cluster.Connect("ClassifiedsDev");
                _mapper = new Mapper(session);
            } catch(NoHostAvailableException e)
            {
                Console.WriteLine("Caught No Host Available Exception");
                foreach(var error in e.Errors)
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }

        public async Task<ChatConnection> CreateConnection(string connId, string userId)
        {
            var conn = new ChatConnection
            {
                ConnId = connId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            await _mapper.InsertIfNotExistsAsync(conn, "insert");
            return conn;
        }

        public async Task<ChatConnection> FindConnectionById(string connId)
        {
            return await _mapper.SingleAsync<ChatConnection>("WHERE connid = ?", connId);
        }

        public async Task DeleteConnection(string connId)
        {
            var conn = FindConnectionById(connId);
            await _mapper.DeleteAsync(conn);
        }

        public async Task<List<ChatConversation>> GetConversations(string userId)
        {
            return (await _mapper.FetchAsync<ChatConversation>("WHERE userid = ?", userId)).ToList();
        }

        public async Task CreateMessage(ChatMessage chatMessage)
        {
            var userName = await GetUserName(chatMessage.RecipientId);
            var recipientName = await GetUserName(chatMessage.SenderId);
            var conversation = new ChatConversation()
            {
                UserId = chatMessage.SenderId,
                RecipientId = chatMessage.RecipientId,
                RecipientLabel = userName
            };
            var conversationInverse = new ChatConversation()
            {
                UserId = chatMessage.RecipientId,
                RecipientId = chatMessage.SenderId,
                RecipientLabel = recipientName
            };
            await _mapper.InsertIfNotExistsAsync(conversation, "insert");
            await _mapper.InsertIfNotExistsAsync(conversationInverse, "insert");
            await _mapper.InsertAsync(chatMessage, "insert");
            await PushMessage(chatMessage);
        }

        public async Task<List<ChatMessage>> GetMessages(string userId, string recipientId)
        {
            var sent = await _mapper.FetchAsync<ChatMessage>("WHERE senderid = ? AND recipientid = ?", userId, recipientId);
            var received = await _mapper.FetchAsync<ChatMessage>("WHERE senderid = ? AND recipientid = ?", recipientId, userId);
            var messages = new List<ChatMessage>();
            foreach(var msg in sent)
            {
                messages.Add(msg);
            }
            foreach (var msg in received)
            {
                messages.Add(msg);
            }
            return messages;
        }

        public async Task PushMessage(ChatMessage chatMessage)
        {
            var connections = await _mapper.FetchAsync<ChatConnection>("WHERE userid = ?", chatMessage.SenderId);
            var connections2 = await _mapper.FetchAsync<ChatConnection>("WHERE userid = ?", chatMessage.RecipientId);
            List<Task> tasks = new List<Task>();
            foreach(var conn in connections)
            {
                tasks.Add(PostMessage(conn.ConnId, chatMessage));
            }
            foreach (var conn in connections2)
            {
                tasks.Add(PostMessage(conn.ConnId, chatMessage));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public async Task PostMessage(string connId, ChatMessage chatMessage)
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(chatMessage, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            }));
            var stream = new MemoryStream(byteArray);
            try
            {
                var response = await _apiClient.GetConnectionAsync(new GetConnectionRequest
                {
                    ConnectionId = connId
                });
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    var response2 = await _apiClient.PostToConnectionAsync(new PostToConnectionRequest
                    {
                        ConnectionId = connId,
                        Data = stream
                    });
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Exception posting message to connection ${connId} error: {e.ToString()}");
            }
        }

        public async Task<string> GetUserName(string userId)
        {
            /*var user = await _oktaClient.Users.GetUserAsync(userId);
            return new PublicUserProfile() { Id = user.Id, UserName = user.Profile.Login };*/
            var response = await _cognitoClient.ListUsersAsync(new ListUsersRequest
            {
                Filter = $"sub=\"{userId}\"",
                Limit = 1,
                UserPoolId = "us-east-1_z8PhK3D8V"
                // AttributesToGet = new[] { "username", "sub" }
            });
            /*if (response.HttpStatusCode.ToString() != "200")
            {
                // _logger.LogDebug("GetPublicUserProfile Error");
            }*/
            var user = response.Users.FirstOrDefault();
            /*if (user == null)
            {
                return NotFound();
            }*/
            return user.Username;
        }
    }
}
