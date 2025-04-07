using Microsoft.AspNetCore.SignalR;

namespace Quizlet_App_Server.Src
{
    public class WebRTCHub : Hub
    {
        private readonly ILogger<WebRTCHub> _logger;

        private static Dictionary<string, string> _connections = new Dictionary<string, string>();

        public WebRTCHub(ILogger<WebRTCHub> logger)
        {
            _logger = logger;
        }

        // 📌 Xử lý khi user kết nối
        public override async Task OnConnectedAsync()
        {
            string userId = Context.ConnectionId;
            _connections[userId] = Context.ConnectionId;
            await Clients.All.SendAsync("UserConnected", userId);
            _logger.LogInformation($"User connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // 📌 Xử lý khi user rời khỏi
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.ConnectionId;
            _connections.Remove(userId);
            await Clients.All.SendAsync("UserDisconnected", userId);
            _logger.LogInformation($"User disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // 📌 Gửi tín hiệu WebRTC (SDP Offer, SDP Answer, ICE Candidate)
        public async Task SendSignal(string type, string sender, string receiver, string data)
        {
            if (_connections.ContainsKey(receiver))
            {
                await Clients.Client(_connections[receiver]).SendAsync("ReceiveSignal", type, sender, data);
                _logger.LogInformation($"Sent WebRTC signal {type} from {sender} to {receiver}");
            }
        }
        public async Task JoinCall(string userId)
        {
            _connections[userId] = Context.ConnectionId;
            await Clients.Others.SendAsync("UserJoined", userId);
        }
    }
}
