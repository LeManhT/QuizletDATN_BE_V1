using MongoDB.Driver;
using Quizlet_App_Server.Utility;
using Quizlet_App_Server.Src.Features.Social.Models;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class GroupService
    {
        private readonly IMongoCollection<Conversation> _conversationCollection;
        private readonly IMongoCollection<Message> _messageCollection;
        protected readonly IMongoClient client;
        private readonly IConfiguration _config;
        private readonly ILogger<GroupService> _logger;

        public GroupService(IMongoClient client, IConfiguration config, ILogger<GroupService> logger)
        {
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            _conversationCollection = database.GetCollection<Conversation>("conversation");
            _messageCollection = database.GetCollection<Message>("messages");
            this.client = client;
            this._config = config;
            this._logger = logger;
        }

        public async Task<Conversation> CreateGroupAsync(Conversation group)
        {
            try
            {
                //// Ensure required fields are set
                //if (string.IsNullOrEmpty(group.Id))
                //{
                //    group = group with { Id = GenerateObjectId() };
                //}

                // Set timestamps
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                group = group with
                {
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    IsActive = true
                };

                _logger.LogInformation($"Creating new group: {group.Name} with ID: {group.Id}");
                await _conversationCollection.InsertOneAsync(group);
                _logger.LogInformation($"Group created successfully: {group.Id}");
                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating group: {ex.Message}");
                throw;
            }
        }

        public async Task<Conversation> GetGroupByIdAsync(string groupId)
        {
            try
            {
                _logger.LogInformation($"Getting group with ID: {groupId}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, groupId);
                var group = await _conversationCollection.Find(filter).FirstOrDefaultAsync();

                if (group == null)
                {
                    _logger.LogWarning($"Group not found with ID: {groupId}");
                    throw new KeyNotFoundException($"Group not found with ID: {groupId}");
                }

                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving group: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Conversation>> GetUserGroupsAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"Getting groups for user: {userId}");
                var filter = Builders<Conversation>.Filter.ElemMatch(g => g.Members,
                    m => m.UserId == userId);

                return await _conversationCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user groups: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AddMemberToGroupAsync(string groupId, GroupMember member)
        {
            try
            {
                _logger.LogInformation($"Adding member {member.UserId} to group: {groupId}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, groupId);
                var group = await _conversationCollection.Find(filter).FirstOrDefaultAsync();

                if (group == null)
                {
                    _logger.LogWarning($"Group not found with ID: {groupId}");
                    return false;
                }

                // Check if user is already a member
                if (group.Members.Any(m => m.UserId == member.UserId))
                {
                    _logger.LogWarning($"User {member.UserId} is already a member of group {groupId}");
                    return false;
                }

                // Add member to group
                var update = Builders<Conversation>.Update
                  .Push(g => g.Members, member)
                  .Set(g => g.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var result = await _conversationCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding member to group: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromGroupAsync(string groupId, string userId)
        {
            try
            {
                _logger.LogInformation($"Removing member {userId} from group {groupId}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, groupId);
                var group = await _conversationCollection.Find(filter).FirstOrDefaultAsync();

                if (group == null)
                {
                    _logger.LogWarning($"Group not found with ID: {groupId}");
                    return false;
                }

                if (!group.Members.Any(m => m.UserId == userId))
                {
                    _logger.LogWarning($"User {userId} is not a member of group {groupId}");
                    return false;
                }

                var update = Builders<Conversation>.Update
                    .PullFilter(g => g.Members, m => m.UserId == userId)
                    //.Set(g => g.Members.Count, group.Members.Count - 1)
                    .Set(g => g.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var result = await _conversationCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing member from group: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Message>> GetGroupMessagesAsync(string groupId, int limit = 50, long? beforeTimestamp = null)
        {
            try
            {
                _logger.LogInformation($"Getting messages for group: {groupId}");
                var filterBuilder = Builders<Message>.Filter;
                var filter = filterBuilder.Eq(m => m.ConversationId, groupId);

                if (beforeTimestamp.HasValue)
                {
                    filter = filterBuilder.And(filter, filterBuilder.Lt(m => m.Timestamp, beforeTimestamp.Value));
                }

                return await _messageCollection
                    .Find(filter)
                    .SortByDescending(m => m.Timestamp)
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving group messages: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateGroupAsync(Conversation group)
        {
            try
            {
                _logger.LogInformation($"Updating group: {group.Id}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, group.Id);

                // Update timestamp
                group = group with { UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

                var result = await _conversationCollection.ReplaceOneAsync(filter, group);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating group: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteGroupAsync(string groupId)
        {
            try
            {
                _logger.LogInformation($"Deactivating group: {groupId}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, groupId);
                var update = Builders<Conversation>.Update
                    .Set(g => g.IsActive, false)
                    .Set(g => g.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var result = await _conversationCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating group: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GroupMember>> GetGroupMembersAsync(string groupId)
        {
            try
            {
                _logger.LogInformation($"Getting members for group: {groupId}");
                var filter = Builders<Conversation>.Filter.Eq(g => g.Id, groupId);
                var group = await _conversationCollection.Find(filter).FirstOrDefaultAsync();

                if (group == null)
                {
                    _logger.LogWarning($"Group not found with ID: {groupId}");
                    throw new KeyNotFoundException($"Group not found with ID: {groupId}");
                }

                return group.Members;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving group members: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateGroupMemberRoleAsync(string groupId, string userId, string newRole)
        {
            try
            {
                _logger.LogInformation($"Updating role for user {userId} in group {groupId} to {newRole}");
                var filter = Builders<Conversation>.Filter.And(
                    Builders<Conversation>.Filter.Eq(g => g.Id, groupId),
                    Builders<Conversation>.Filter.ElemMatch(g => g.Members, m => m.UserId == userId)
                );

                var update = Builders<Conversation>.Update
                    .Set(g => g.Members[-1].Role, newRole)
                    .Set(g => g.UpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var result = await _conversationCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating member role: {ex.Message}");
                throw;
            }
        }
    }
}
