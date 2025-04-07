
using Microsoft.AspNetCore.SignalR;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Controller;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;
using System.Threading.Tasks;
namespace Quizlet_App_Server.Src
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;
        private readonly ILogger<ChatHub> logger;

        public ChatHub(MessageService messageService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            this.logger = logger;
            logger.LogInformation("ChatHub initialized with MessageService.");
        }
        public async Task SendMessage(string userId, MessageDTO message)
        {
            try
            {
                logger.LogInformation("Informationnnnnnn : ", $"Received message from {userId}: {message.Content}");
                await _messageService.SaveMessageAsync(message);
                await Clients.All.SendAsync("ReceiveMessage", userId, message);
                logger.LogInformation("Informationnnnnnn : ", "Message saved and broadcasted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            logger.LogInformation($"Client connected: {Context.ConnectionId}");
            //Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            logger.LogInformation("Info=","Client disconnected: {Context.ConnectionId}");
        }

        public async Task TestConnection()
        {
            await Clients.All.SendAsync("ReceiveMessage", "Server", "Test message");
        }


    }

}
