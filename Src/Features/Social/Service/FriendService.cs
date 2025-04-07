using MongoDB.Driver;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class FriendService
    {
        private readonly IMongoCollection<FriendRequest> _friendRequestCollection;
        private readonly IMongoCollection<Friend> _friendCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly ILogger<FriendService> _logger;

        public FriendService(IMongoClient mongoClient, ILogger<FriendService> logger)
        {
            var database = mongoClient.GetDatabase(VariableConfig.DatabaseName);
            _friendRequestCollection = database.GetCollection<FriendRequest>("friendRequest");
            _friendCollection = database.GetCollection<Friend>("friends");
            _userCollection = database.GetCollection<User>("users");
            _logger = logger;
        }

        // Gửi yêu cầu kết bạn
        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
        {
            try
            {
                var existingRequest = await _friendRequestCollection.Find(f =>
                    (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                    (f.SenderId == receiverId && f.ReceiverId == senderId)
                ).FirstOrDefaultAsync();

                if (existingRequest != null) return false;

                var newRequest = new FriendRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Status = "pending",
                    CreatedAt = TimeHelper.UnixTimeNow
                };

                await _friendRequestCollection.InsertOneAsync(newRequest);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send friend request");
                return false;
            }
        }

        // Chấp nhận lời mời kết bạn
        public async Task<bool> AcceptFriendRequestAsync(string requestId)
        {
            var request = await _friendRequestCollection.Find(f => f.Id == requestId && f.Status == "pending").FirstOrDefaultAsync();
            if (request == null) return false;

            var newFriend = new Friend
            {
                UserIds = new List<string> { request.SenderId, request.ReceiverId },
                CreatedAt = TimeHelper.UnixTimeNow
            };

            await _friendCollection.InsertOneAsync(newFriend);
            await _friendRequestCollection.DeleteOneAsync(f => f.Id == requestId);

            return true;
        }


        // Từ chối lời mời kết bạn
        public async Task<bool> RemoveFriendRequestAsync(string requestId)
        {
            var result = await _friendRequestCollection.DeleteOneAsync(f => f.Id == requestId);
            return result.DeletedCount > 0;
        }

        // Xóa bạn bè
        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            var filter = Builders<Friend>.Filter.All(f => f.UserIds, new[] { userId, friendId });
            var result = await _friendCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }


        // Lấy danh sách bạn bè
        //public async Task<List<string>> GetFriendsAsync(string userId)
        //{
        //    var friendships = await _friendCollection.Find(f => f.UserIds.Contains(userId)).ToListAsync();

        //    return friendships.Select(f => f.UserIds.First(id => id != userId)).ToList();
        //}

        public async Task<List<User>> GetFriendsAsync(string userId)
        {
            var friendships = await _friendCollection.Find(f => f.UserIds.Contains(userId)).ToListAsync();

            var friendIds = friendships.Select(f => f.UserIds.First(id => id != userId)).ToList();

            var friends = await _userCollection.Find(u => friendIds.Contains(u.Id)).ToListAsync();

            return friends;
        }

        // Hủy lời mời kết bạn
        public async Task<bool> CancelFriendRequestAsync(string senderId, string receiverId)
        {
            try
            {
                var result = await _friendRequestCollection.DeleteOneAsync(f =>
                    f.SenderId == senderId && f.ReceiverId == receiverId && f.Status == "pending");

                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling friend request");
                return false;
            }
        }

        // Lấy danh sách lời mời kết bạn mới nhận được
        public async Task<List<FriendRequest>> GetReceivedFriendRequestsAsync(string receiverId)
        {
            var requests = await _friendRequestCollection
                .Find(f => f.ReceiverId == receiverId && f.Status == "pending")
                .ToListAsync();

            var senderIds = requests.Select(r => r.SenderId).ToList();
            var senders = await _userCollection.Find(u => senderIds.Contains(u.Id)).ToListAsync();
            var friends = await _friendCollection.Find(f => f.UserIds.Contains(receiverId)).ToListAsync();

            var friendIds = friends.SelectMany(f => f.UserIds).Distinct().Where(id => id != receiverId).ToHashSet();

            var result = requests.Select(request =>
            {
                var sender = senders.FirstOrDefault(u => u.Id == request.SenderId);
                var senderName = sender?.UserName ?? "Unknown";

                var senderFriends = _friendCollection
                    .Find(f => f.UserIds.Contains(request.SenderId))
                    .ToList()
                    .SelectMany(f => f.UserIds)
                    .Distinct()
                    .Where(id => id != request.SenderId)
                    .ToHashSet();

                int mutualFriends = senderFriends.Intersect(friendIds).Count();

                return new FriendRequest
                {
                    Id = request.Id,
                    SenderId = request.SenderId,
                    ReceiverId = receiverId,
                    SenderName = senderName,
                    CreatedAt = request.CreatedAt,
                    MutualFriends = mutualFriends
                };
            }).ToList();

            return result;
        }




    }
}
