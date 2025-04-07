using MongoDB.Driver;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class ChatHistoryService
    {
        private readonly IMongoCollection<ChatBotHistory> _chatHistoryCollection;

        public ChatHistoryService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            _chatHistoryCollection = database.GetCollection<ChatBotHistory>("ChatHistories");
        }

        // Lưu tin nhắn vào session chat
        public async Task SaveChatMessageAsync(string userId, string sessionId, ChatMessage message)
        {
            var filter = Builders<ChatBotHistory>.Filter.And(
                Builders<ChatBotHistory>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatBotHistory>.Filter.Eq(c => c.SessionId, sessionId)
            );

            var update = Builders<ChatBotHistory>.Update
                .Push(c => c.Messages, message)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _chatHistoryCollection.UpdateOneAsync(filter, update);

            // Nếu không tìm thấy sessionId thì tạo mới
            if (result.MatchedCount == 0)
            {
                var chatHistory = new ChatBotHistory
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Messages = new List<ChatMessage> { message }
                };

                await _chatHistoryCollection.InsertOneAsync(chatHistory);
            }
        }

        // Lấy lịch sử hội thoại theo userId
        public async Task<List<ChatBotHistory>> GetChatHistoryAsync(string userId)
        {
            return await _chatHistoryCollection.Find(c => c.UserId == userId)
                                               .SortByDescending(c => c.UpdatedAt)
                                               .ToListAsync();
        }

        // Xóa lịch sử chat theo userId
        public async Task<bool> DeleteChatHistoryAsync(string userId)
        {
            var result = await _chatHistoryCollection.DeleteManyAsync(c => c.UserId == userId);
            return result.DeletedCount > 0;
        }
    }
}
