using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Quizlet_App_Server.Src.Features.Social.Service;

namespace Quizlet_App_Server.Src
{
    public class FriendHub : Hub
    {
        private readonly FriendService _friendService;
        private readonly ILogger<FriendHub> _logger;

        public FriendHub(FriendService friendService, ILogger<FriendHub> logger)
        {
            _friendService = friendService;
            _logger = logger;
            _logger.LogInformation("FriendHub initialized.");
        }

        public async Task SendFriendRequest(string senderId, string receiverId)
        {
            var success = await _friendService.SendFriendRequestAsync(senderId, receiverId);
            if (success)
            {
                await Clients.User(receiverId).SendAsync("ReceiveFriendRequest", senderId);
            }
        }

        public async Task AcceptFriendRequest(string requestId)
        {
            var success = await _friendService.AcceptFriendRequestAsync(requestId);
            _logger.LogInformation("New AcceptFriendRequest $requestId", requestId);
            if (success)
            {
                _logger.LogInformation("Success", requestId);
                await Clients.All.SendAsync("AcceptFriendRequest", requestId);
            }
        }

        public async Task CancelFriendRequest([FromQuery] string senderId, [FromQuery] string receiverId)
        {
            var success = await _friendService.CancelFriendRequestAsync(senderId, receiverId);
            if (success)
            {
                await Clients.User(receiverId).SendAsync("CancelFriendRequestAsync", senderId);
            }
        }
    }
}
